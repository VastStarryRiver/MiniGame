/**
 * \file
 */

#ifndef __MONO_DEBUGGER_AGENT_H__
#define __MONO_DEBUGGER_AGENT_H__

#ifndef RUNTIME_IL2CPP
#include "mini.h"
#endif

#include <mono/utils/mono-stack-unwinding.h>

#ifdef RUNTIME_IL2CPP
typedef MonoStackFrameInfo StackFrameInfo;
typedef struct Il2CppSequencePoint Il2CppSequencePoint;
#include <mono/metadata/threads-types.h>
#endif // RUNTIME_IL2CPP

#define MONO_DBG_CALLBACKS_VERSION (3)
// 2. debug_log parameters changed from MonoString* to MonoStringHandle
// 3. debug_log parameters changed from MonoStringHandle back to MonoString*

struct _MonoDebuggerCallbacks {
	int version;
	void (*parse_options) (char *options);
	void (*init) (void);
	void (*breakpoint_hit) (void *sigctx);
	void (*single_step_event) (void *sigctx);
	void (*single_step_from_context) (MonoContext *ctx, Il2CppSequencePoint* sequencePoint);
	void (*breakpoint_from_context) (MonoContext *ctx);
	void (*free_domain_info) (MonoDomain *domain);
	void (*unhandled_exception) (MonoException *exc);
	void (*handle_exception) (MonoException *exc, MonoContext *throw_ctx,
							  MonoContext *catch_ctx, StackFrameInfo *catch_frame);
	void (*begin_exception_filter) (MonoException *exc, MonoContext *ctx, MonoContext *orig_ctx);
	void (*end_exception_filter) (MonoException *exc, MonoContext *ctx, MonoContext *orig_ctx);
	void (*user_break) (void);
	void (*debug_log) (int level, MonoString *category, MonoString *message);
	gboolean (*debug_log_is_enabled) (void);
	void (*send_crash) (char *json_dump, MonoStackHash *hashes, int pause);
};

typedef struct _DebuggerTlsData DebuggerTlsData;

MONO_API void
mono_debugger_agent_init (void);

MONO_API void
mono_debugger_agent_parse_options (char *options);

void
mono_debugger_agent_stub_init (void);

MONO_API gboolean
mono_debugger_agent_transport_handshake (void);

#define HEADER_LENGTH 11

typedef enum {
    CMD_SET_VM = 1,
    CMD_SET_OBJECT_REF = 9,
    CMD_SET_STRING_REF = 10,
    CMD_SET_THREAD = 11,
    CMD_SET_ARRAY_REF = 13,
    CMD_SET_EVENT_REQUEST = 15,
    CMD_SET_STACK_FRAME = 16,
    CMD_SET_APPDOMAIN = 20,
    CMD_SET_ASSEMBLY = 21,
    CMD_SET_METHOD = 22,
    CMD_SET_TYPE = 23,
    CMD_SET_MODULE = 24,
    CMD_SET_FIELD = 25,
    CMD_SET_EVENT = 64,
    CMD_SET_POINTER = 65
} CommandSet;

typedef enum {
    CMD_VM_VERSION = 1,
    CMD_VM_ALL_THREADS = 2,
    CMD_VM_SUSPEND = 3,
    CMD_VM_RESUME = 4,
    CMD_VM_EXIT = 5,
    CMD_VM_DISPOSE = 6,
    CMD_VM_INVOKE_METHOD = 7,
    CMD_VM_SET_PROTOCOL_VERSION = 8,
    CMD_VM_ABORT_INVOKE = 9,
    CMD_VM_SET_KEEPALIVE = 10,
    CMD_VM_GET_TYPES_FOR_SOURCE_FILE = 11,
    CMD_VM_GET_TYPES = 12,
    CMD_VM_INVOKE_METHODS = 13,
    CMD_VM_START_BUFFERING = 14,
    CMD_VM_STOP_BUFFERING = 15
} CmdVM;

typedef struct {
    guint8 *buf, *p, *end;
} Buffer;

typedef enum {
    ERR_NONE = 0,
    ERR_INVALID_OBJECT = 20,
    ERR_INVALID_FIELDID = 25,
    ERR_INVALID_FRAMEID = 30,
    ERR_NOT_IMPLEMENTED = 100,
    ERR_NOT_SUSPENDED = 101,
    ERR_INVALID_ARGUMENT = 102,
    ERR_UNLOADED = 103,
    ERR_NO_INVOCATION = 104,
    ERR_ABSENT_INFORMATION = 105,
    ERR_NO_SEQ_POINT_AT_IL_OFFSET = 106,
    ERR_INVOKE_ABORTED = 107,
    ERR_LOADER_ERROR = 200, /*XXX extend the protocol to pass this information down the pipe */
} ErrorCode;

typedef enum {
    ID_ASSEMBLY = 0,
    ID_MODULE = 1,
    ID_TYPE = 2,
    ID_METHOD = 3,
    ID_FIELD = 4,
    ID_DOMAIN = 5,
    ID_PROPERTY = 6,
    ID_NUM
} IdType;

typedef struct {
    /* Maps runtime structure -> Id */
    /* Protected by the dbg lock */
    GHashTable *val_to_id [ID_NUM];
    /* Classes whose class load event has been sent */
    /* Protected by the loader lock */
    GHashTable *loaded_classes;
    /* Maps MonoClass->GPtrArray of file names */
    GHashTable *source_files;
    /* Maps source file basename -> GSList of classes */
    GHashTable *source_file_to_class;
    /* Same with ignore-case */
    GHashTable *source_file_to_class_ignorecase;
} AgentDomainInfo;

typedef struct {
    gboolean enabled;
    char *transport;
    char *address;
    int log_level;
    char *log_file;
    gboolean suspend;
    gboolean server;
    gboolean onuncaught;
    GSList *onthrow;
    int timeout;
    char *launch;
    gboolean embedding;
    gboolean defer;
    int keepalive;
    gboolean setpgid;
} AgentConfig;

typedef void (*MonoDebuggerAttachFunc)(gboolean attached);

typedef struct ReplyPacket {
    int id;
    int error;
    Buffer *data;
} ReplyPacket;

/*
 * Wire Protocol definitions
 */

#define MAJOR_VERSION 2
#define MINOR_VERSION 58

#include "debugger-agent-web.h"

#ifdef RUNTIME_IL2CPP
typedef struct _MonoProfiler MonoProfiler;
typedef struct _MonoProfilerDesc *MonoProfilerHandle;
#endif

#endif
