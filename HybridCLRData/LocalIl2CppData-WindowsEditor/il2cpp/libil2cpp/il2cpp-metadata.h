#pragma once

#include "il2cpp-config.h"
#include <stdint.h>
#include "il2cpp-tokentype.h"

#if SLIM_GLOBAL_METADATA_FILE
typedef uint16_t slim_int32_t;
#define slim_int32_t_invalid UINT16_MAX;
#else
typedef int32_t slim_int32_t;
#define slim_int32_t_invalid  -1;
#endif

typedef slim_int32_t TypeIndex;
typedef slim_int32_t TypeDefinitionIndex;
typedef int32_t FieldIndex;
typedef int32_t DefaultValueIndex;
typedef int32_t DefaultValueDataIndex;
typedef int32_t CustomAttributeIndex;
typedef int32_t ParameterIndex;
typedef int32_t MethodIndex;
typedef int32_t GenericMethodIndex;
typedef slim_int32_t PropertyIndex;
typedef slim_int32_t EventIndex;
typedef slim_int32_t GenericContainerIndex;
typedef slim_int32_t GenericParameterIndex;
typedef int16_t GenericParameterConstraintIndex;
typedef slim_int32_t NestedTypeIndex;
typedef slim_int32_t InterfacesIndex;
typedef int32_t VTableIndex;
typedef int32_t RGCTXIndex;
typedef int32_t StringIndex;
typedef int32_t StringLiteralIndex;
typedef int32_t GenericInstIndex;
typedef int32_t ImageIndex;
typedef int32_t AssemblyIndex;
typedef int32_t InteropDataIndex;

// "Relative" indexes - based on their parent
typedef int32_t TypeFieldIndex;
typedef int32_t TypeMethodIndex;
typedef int32_t MethodParameterIndex;
typedef int32_t TypePropertyIndex;
typedef int32_t TypeEventIndex;
typedef int32_t TypeInterfaceIndex;
typedef int32_t TypeNestedTypeIndex;
typedef int32_t TypeInterfaceOffsetIndex;
typedef int32_t GenericContainerParameterIndex;
typedef int32_t AssemblyTypeIndex;
typedef int32_t AssemblyExportedTypeIndex;

static const TypeIndex kTypeIndexInvalid = slim_int32_t_invalid;
static const TypeDefinitionIndex kTypeDefinitionIndexInvalid = slim_int32_t_invalid;
static const DefaultValueDataIndex kDefaultValueIndexNull = -1;
static const CustomAttributeIndex kCustomAttributeIndexInvalid = -1;
static const EventIndex kEventIndexInvalid = slim_int32_t_invalid;
static const FieldIndex kFieldIndexInvalid = -1;
static const MethodIndex kMethodIndexInvalid = -1;
static const PropertyIndex kPropertyIndexInvalid = slim_int32_t_invalid;
static const GenericContainerIndex kGenericContainerIndexInvalid = slim_int32_t_invalid;
static const GenericParameterIndex kGenericParameterIndexInvalid = slim_int32_t_invalid;
static const RGCTXIndex kRGCTXIndexInvalid = -1;
static const StringLiteralIndex kStringLiteralIndexInvalid = -1;
static const InteropDataIndex kInteropDataIndexInvalid = -1;

#define PUBLIC_KEY_BYTE_LENGTH 8
static const int kPublicKeyByteLength = PUBLIC_KEY_BYTE_LENGTH;

typedef struct Il2CppMethodSpec
{
    MethodIndex methodDefinitionIndex;
    GenericInstIndex classIndexIndex;
    GenericInstIndex methodIndexIndex;
} Il2CppMethodSpec;

typedef enum Il2CppRGCTXDataType
{
    IL2CPP_RGCTX_DATA_INVALID,
    IL2CPP_RGCTX_DATA_TYPE,
    IL2CPP_RGCTX_DATA_CLASS,
    IL2CPP_RGCTX_DATA_METHOD,
    IL2CPP_RGCTX_DATA_ARRAY,
    IL2CPP_RGCTX_DATA_CONSTRAINED,
} Il2CppRGCTXDataType;

typedef union Il2CppRGCTXDefinitionData
{
    int32_t rgctxDataDummy;
    MethodIndex __methodIndex;
    TypeIndex __typeIndex;
} Il2CppRGCTXDefinitionData;

typedef struct Il2CppRGCTXConstrainedData
{
    TypeIndex __typeIndex;
    uint32_t __encodedMethodIndex;
} Il2CppRGCTXConstrainedData;

typedef struct Il2CppRGCTXDefinition
{
    Il2CppRGCTXDataType type;
    const void* data;
} Il2CppRGCTXDefinition;

typedef struct
{
    MethodIndex methodIndex;
    MethodIndex invokerIndex;
    MethodIndex adjustorThunkIndex;
} Il2CppGenericMethodIndices;

typedef struct Il2CppGenericMethodFunctionsDefinitions
{
    GenericMethodIndex genericMethodIndex;
    Il2CppGenericMethodIndices indices;
} Il2CppGenericMethodFunctionsDefinitions;

static inline uint32_t GetTokenType(uint32_t token)
{
    return token & 0xFF000000;
}

static inline uint32_t GetTokenRowId(uint32_t token)
{
    return token & 0x00FFFFFF;
}

/* Runtime metadata tokens  */
typedef const struct ___Il2CppMetadataImageHandle* Il2CppMetadataImageHandle;
typedef const struct ___Il2CppMetadataCustomAttributeHandle* Il2CppMetadataCustomAttributeHandle;
typedef const struct ___Il2CppMetadataTypeHandle* Il2CppMetadataTypeHandle;
typedef const struct ___Il2CppMetadataMethodHandle* Il2CppMetadataMethodDefinitionHandle;
typedef const struct ___Il2CppMetadataGenericContainerHandle* Il2CppMetadataGenericContainerHandle;
typedef const struct ___Il2CppMetadataGenericParameterHandle* Il2CppMetadataGenericParameterHandle;
