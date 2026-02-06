//
// Created by xi.he on 2023/12/11.
//

#if IL2CPP_WEIXINMINIGAME_DEBUGGER
extern void JS_web_debugger_worker_create(void);
extern void JS_web_debugger_socket_create (char* addr);
extern int JS_web_debugger_socket_send (char* data, int len);
extern int JS_web_debugger_socket_recv (char* data, int len);
extern void JS_web_debugger_socket_close (void);

void data_packet_queue_pop_and_send(void);
void data_packet_queue_push(int command_set, int command, guint8* buf, int length);
void debugger_web_packet_init(void);
void debugger_web_packet_finish(void);

void debugger_agent_web_main_loop_entry(void);
void debugger_main_loop(void);

gboolean debugger_web_send(Buffer* buf, int len, CommandSet command_set, int command);

typedef enum {
    DBG_STATE_WAIT = 0,
    DBG_STATE_CONNECTED = 1,
    DBG_STATE_ATTACHED = 2,
    DBG_STATE_WORKING = 3,
    DBG_STATE_QUIT = 4,
    DBG_STATE_FAILED = 5,
    DBG_STATE_EXITED = 6,
    DBG_STATE_WORKER_CREATED = 7
} DebuggerMainLoopState;
#endif