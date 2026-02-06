//
// Created by xi.he on 2023/12/11.
//

#include "os/c-api/il2cpp-config-platforms.h"

#if IL2CPP_WEIXINMINIGAME_DEBUGGER
#include <config.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef HAVE_SYS_TYPES_H
#include <sys/types.h>
#endif
#ifdef HAVE_SYS_SELECT_H
#include <sys/select.h>
#endif
#ifdef HAVE_SYS_SOCKET_H
#include <sys/socket.h>
#endif
#ifdef HAVE_NETINET_TCP_H
#include <netinet/tcp.h>
#endif
#ifdef HAVE_NETINET_IN_H
#include <netinet/in.h>
#endif
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include <errno.h>
#include <glib.h>

#ifdef HAVE_PTHREAD_H
#include <pthread.h>
#endif
#ifdef HOST_WIN32
#ifdef _MSC_VER
#include <winsock2.h>
#include <process.h>
#endif
#include <ws2tcpip.h>
#include <windows.h>
#endif

#ifdef HOST_ANDROID
#include <linux/in.h>
#include <linux/tcp.h>
#include <sys/endian.h>
#endif



#include <mono/metadata/mono-debug.h>
#include <mono/metadata/debug-internals.h>
#include <mono/metadata/domain-internals.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-hash-internals.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/threadpool.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/assembly-internals.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/reflection-internals.h>
#include <mono/metadata/w32socket.h>
#include <mono/metadata/w32socket-internals.h>
#include <mono/utils/mono-coop-mutex.h>
#include <mono/utils/mono-coop-semaphore.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/mono-stack-unwinding.h>
#include <mono/utils/mono-time.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/networking.h>
#include <mono/utils/mono-proclib.h>
#include <mono/utils/w32api.h>
#include <mono/utils/mono-logger-internals.h>
#include "debugger-agent.h"
#include "debugger-state-machine.h"

#ifndef RUNTIME_IL2CPP
#include "mini.h"
#include "seq-points.h"
#include "aot-runtime.h"
#include "mini-runtime.h"
#include "interp/interp.h"
#endif

#include "debugger-engine.h"
#include <emscripten.h>
#include "mono/metadata/debug-mono-ppdb.h"
#include "mono/metadata/custom-attrs-internals.h"

#include <mono/utils/mono-os-mutex.h>

#include <fcntl.h>
#include <sys/stat.h>

/* Global */
extern gboolean debugger_thread_exited;
extern MonoCoopMutex debugger_thread_exited_mutex;
extern MonoDebuggerAttachFunc attach_func;
extern MonoCoopCond debugger_thread_exited_cond;
extern int log_level;
extern gboolean vm_start_event_sent, vm_death_event_sent, disconnected;
extern FILE *log_file;
extern gboolean buffer_replies;
extern int major_version, minor_version;
extern gboolean protocol_version_set;
extern MonoNativeThreadId debugger_thread_id;
extern AgentConfig agent_config;
extern gboolean is_debugger_thread (void);

// These functions are defined in debugger-agent.c, but must be called from here, so forward declare them.
void dispose_vm (void);
int decode_int (guint8 *buf, guint8 **endbuf, guint8 *limit);
int decode_byte (guint8 *buf, guint8 **endbuf, guint8 *limit);
const char* cmd_to_string (CommandSet set, int command);
void buffer_init (Buffer *buf, int size);
void buffer_reply_packet (int id, int error, Buffer *data);
gboolean send_reply_packets (int npackets, ReplyPacket *packets);
gboolean send_reply_packet (int id, int error, Buffer *data);
void send_buffered_reply_packets (void);
void process_profiler_event (EventKind event, gpointer arg);
void appdomain_load (MonoProfiler *prof, MonoDomain *domain);
void buffer_free (Buffer *buf);
const char* command_set_to_string (CommandSet command_set);
ErrorCode process_request(CommandSet command_set, int command, int id, guint8 *p, guint8 *end, Buffer* buf, gboolean* no_reply);

// These are used only in debugger-agent-web.c
typedef struct data_packet_node data_packet_node;
typedef struct data_packet_queue data_packet_queue;
struct data_packet_node {
    int commandSet;
    int command;
    int length;
    guint8* buf;
    data_packet_node* next;
};
struct data_packet_queue {
    data_packet_node* head;
    data_packet_node* tail;
};

MonoCoopMutex data_packet_queue_mutex;
data_packet_queue* debugger_data_packet_queue;

int data_packet_send_sync(guint8* buf, int length) {
    int res = -1;
    do {
        res = JS_web_debugger_socket_send(buf, length);
    } while (res == -1);
    return res;
}

gboolean data_packet_queue_ready_to_send(void) {
    if(debugger_data_packet_queue == NULL) {
        return FALSE;
    }

    if(debugger_data_packet_queue -> head == NULL) {
        return FALSE;
    }

    return TRUE;
}

void data_packet_queue_pop_and_send(void) {
    if (!data_packet_queue_ready_to_send())
    {
        return;
    }

     while(debugger_data_packet_queue -> head != NULL) {
        data_packet_node* tmp = debugger_data_packet_queue -> head;
        debugger_data_packet_queue -> head = debugger_data_packet_queue -> head -> next;
        data_packet_send_sync(tmp->buf, tmp->length);
        g_free (tmp->buf);
        g_free (tmp);
    }

    debugger_data_packet_queue -> tail = NULL;
}

int data_packet_recv_sync(guint8* buf, int length, gboolean can_send_async)
{
    int res = -1;
    do {
        if (can_send_async && data_packet_queue_ready_to_send())
        {
            mono_coop_mutex_lock(&data_packet_queue_mutex);
            data_packet_queue_pop_and_send();
            mono_coop_mutex_unlock(&data_packet_queue_mutex);
        }

        res = JS_web_debugger_socket_recv(buf, length);
    } while (res == -1);
    return res;
}

void data_packet_queue_push(int command_set, int command, guint8* buf, int length) {
    data_packet_node* tmp = g_malloc(sizeof(data_packet_node));
    tmp->length = length;
    tmp->commandSet = command_set;
    tmp->command = command;
    tmp->buf = buf;
    tmp->next = NULL;

    if(debugger_data_packet_queue -> tail != NULL)
        debugger_data_packet_queue -> tail -> next = tmp;
    debugger_data_packet_queue -> tail = tmp;
    if(debugger_data_packet_queue -> head == NULL)
        debugger_data_packet_queue -> head = tmp;
}

void debugger_web_packet_init(void) {
    debugger_data_packet_queue = g_malloc(sizeof(data_packet_queue));
    debugger_data_packet_queue->head = NULL;
    debugger_data_packet_queue->tail = NULL;
}

void debugger_web_packet_finish(void) {
    if(debugger_data_packet_queue != NULL) {
        while(debugger_data_packet_queue->head != NULL) {
            data_packet_node* tmp = debugger_data_packet_queue->head;
            debugger_data_packet_queue->head = debugger_data_packet_queue->head->next;
            g_free(tmp->buf);
            g_free(tmp);
        }
        g_free(debugger_data_packet_queue);
    }
}

gboolean debugger_web_send(Buffer* buf, int len, CommandSet command_set, int command) {
    gboolean res;
    if(is_debugger_thread()) {
        data_packet_send_sync(buf->buf, len);
        buffer_free (buf);
        res = TRUE;
    } else {
        mono_coop_mutex_lock(&data_packet_queue_mutex);
        data_packet_queue_push(command_set, command, buf->buf, len);
        mono_coop_mutex_unlock(&data_packet_queue_mutex);
        res = FALSE;
    }
    return res;
}

DebuggerMainLoopState debugger_state;
MonoThread *thread;
gboolean log_each_step;
gboolean debugger_ready_to_work = FALSE;

void debugger_do_main_loop(void) {
    if (!debugger_ready_to_work)
    {
        PRINT_DEBUG_MSG (1, "[dbg] Agent thread started, pid=%p\n", (gpointer) (gsize) mono_native_thread_id_get ());

        log_each_step = g_hasenv ("MONO_DEBUGGER_LOG_AFTER_COMMAND");

        debugger_thread_id = mono_native_thread_id_get ();
#ifdef RUNTIME_IL2CPP
        thread = mono_thread_attach (mono_get_root_domain ());
#endif // RUNTIME_IL2CPP
        MonoInternalThread *internal = mono_thread_internal_current ();

#ifdef RUNTIME_IL2CPP
        internal->debugger_thread = 1;
#endif // RUNTIME_IL2CPP

        mono_thread_set_name_constant_ignore_error (internal, "Debugger agent", MonoSetThreadNameFlag_Permanent);

        internal->state |= ThreadState_Background;
        internal->flags |= MONO_THREAD_FLAG_DONT_MANAGE;
        mono_coop_mutex_init(&data_packet_queue_mutex);
        debugger_web_packet_init();

        JS_web_debugger_worker_create();
        debugger_state = DBG_STATE_WORKER_CREATED;

        //give the debugger thread one frame to initialize the web socket worker thread
        debugger_ready_to_work = TRUE;
        return;
    }

    //stop the main loop because we are now running a while loop instead
    emscripten_cancel_main_loop();
    debugger_main_loop();
}

void debugger_agent_web_main_loop_entry(void) {
    emscripten_set_main_loop(debugger_do_main_loop, 1, 1);
}

void debugger_main_loop(void) {
    gboolean attach_failed = FALSE;
    int res, len, id, flags, command;
    CommandSet command_set;
    guint8 header [HEADER_LENGTH];
    guint8 *data, *p, *end;
    Buffer buf;
    ErrorCode err;
    gboolean no_reply;

    MonoDomain* domain;
    AgentDomainInfo *info;

    char handshake_msg [128];
    guint8 handshake_buf [128];

    while (debugger_state != DBG_STATE_EXITED)
    {
        if (debugger_state == DBG_STATE_WORKER_CREATED)
        {
            JS_web_debugger_socket_create((char*) agent_config.address);
            debugger_state = DBG_STATE_CONNECTED;
        }
        else if (debugger_state == DBG_STATE_QUIT || debugger_state == DBG_STATE_FAILED)
        {
            mono_set_is_debugger_attached (FALSE);
            JS_web_debugger_socket_close();
#ifdef RUNTIME_IL2CPP
            mono_free_method_signatures();
#endif
            debugger_web_packet_finish(); // release data packet queue

            if (attach_func)
                attach_func (FALSE);

            mono_coop_mutex_lock (&debugger_thread_exited_mutex);
            debugger_thread_exited = TRUE;
            mono_coop_cond_signal (&debugger_thread_exited_cond);
            mono_coop_mutex_unlock (&debugger_thread_exited_mutex);

            PRINT_DEBUG_MSG (1, "[dbg] Debugger thread exited.\n");

            if (!attach_failed && command_set == CMD_SET_VM && command == CMD_VM_DISPOSE && !(vm_death_event_sent || mono_runtime_is_shutting_down ())) {
                PRINT_MSG("hit here on stop debug\n");
            }

#ifdef RUNTIME_IL2CPP
            mono_thread_detach (thread);
#endif // RUNTIME_IL2CPP

            debugger_state = DBG_STATE_EXITED;
        }
        else if (debugger_state == DBG_STATE_CONNECTED)
        {
            disconnected = TRUE;

            /* Write handshake message */
            sprintf (handshake_msg, "DWP-Handshake");
            /* Read answer */
            res = data_packet_recv_sync(handshake_buf, strlen (handshake_msg), FALSE);
            if((res != strlen (handshake_msg)) || (memcmp (handshake_buf, handshake_msg, strlen (handshake_msg)) != 0)){
                attach_failed = TRUE;
                debugger_state = DBG_STATE_FAILED;
                continue;
            }

            res = data_packet_send_sync(handshake_msg, strlen (handshake_msg));
            major_version = MAJOR_VERSION;
            minor_version = MINOR_VERSION;
            protocol_version_set = FALSE;
            disconnected = FALSE;
            debugger_state = DBG_STATE_ATTACHED;
            mono_set_is_debugger_attached (TRUE);
            if (attach_func)
                attach_func (TRUE);

#ifdef RUNTIME_IL2CPP
            /* Send start event to the other side */
            process_profiler_event (EVENT_KIND_VM_START, mono_thread_get_main ());
            domain = mono_get_root_domain();
            appdomain_load(NULL, domain);
            info = VM_DOMAIN_GET_AGENT_INFO(domain);
            void *iter = NULL;
            MonoClass *klass;
            while(klass = il2cpp_iterate_loaded_classes(&iter))
            {
                g_hash_table_insert(info->loaded_classes, klass, klass);
            }
#endif
            debugger_state = DBG_STATE_WORKING;
        }
        else if (debugger_state == DBG_STATE_WORKING)
        {
            res = data_packet_recv_sync(header, HEADER_LENGTH, TRUE);
            if (res != HEADER_LENGTH) {
                command_set = CMD_SET_VM;
                command = CMD_VM_DISPOSE;
                dispose_vm ();
                debugger_state = DBG_STATE_FAILED;
                continue;
            }
            else
            {
                p = header;
                end = header + HEADER_LENGTH;

                len = decode_int (p, &p, end);
                id = decode_int (p, &p, end);
                flags = decode_byte (p, &p, end);
                command_set = (CommandSet)decode_byte (p, &p, end);
                command = decode_byte (p, &p, end);
            }

            g_assert (flags == 0);
            const char *cmd_str;
            char cmd_num [256];

            cmd_str = cmd_to_string (command_set, command);
            if (!cmd_str) {
                sprintf (cmd_num, "%d", command);
                cmd_str = cmd_num;
            }

            if (log_level) {
                PRINT_DEBUG_MSG (1, "[dbg] Command %s(%s) [%d][at=%lx].\n", command_set_to_string (command_set), cmd_str, id, (long)mono_100ns_ticks () / 10000);
            }
            
            data = (guint8 *)g_malloc (len - HEADER_LENGTH);

            if (len - HEADER_LENGTH > 0)
            {
                res = data_packet_recv_sync(data, len - HEADER_LENGTH, TRUE);
                if (res != len - HEADER_LENGTH) {
                    debugger_state = DBG_STATE_FAILED;
                    continue;
                }
            }

            p = data;

            end = data + (len - HEADER_LENGTH);

            buffer_init (&buf, 128);

            err = ERR_NONE;
            no_reply = FALSE;

            /* Process the request */
            err = process_request(command_set, command, id, p, end, &buf, &no_reply);

            if (command_set == CMD_SET_VM && command == CMD_VM_START_BUFFERING) {
                buffer_replies = TRUE;
            }

            if (!no_reply) {
                if (buffer_replies) {
                    buffer_reply_packet (id, err, &buf);
                } else {
                    send_reply_packet (id, err, &buf);
                }
            }

            if (err == ERR_NONE && command_set == CMD_SET_VM && command == CMD_VM_STOP_BUFFERING) {
                send_buffered_reply_packets ();
                buffer_replies = FALSE;
            }

            g_free (data);
            buffer_free (&buf);

            if (log_each_step) {
                char *debugger_log = mono_debugger_state_str ();
                if (debugger_log) {
                    g_free (debugger_log);
                }
            }

            if (command_set == CMD_SET_VM && (command == CMD_VM_DISPOSE || command == CMD_VM_EXIT)){
                debugger_state = DBG_STATE_QUIT;
                continue;
            }
        }
    }
}
#endif //IL2CPP_WEIXINMINIGAME_DEBUGGER

