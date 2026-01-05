#if UNITY_WEBGL && MINIGAME_SUBPLATFORM_WEIXIN
using YooAsset;

internal partial class WXFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly WechatFileSystem _fileSystem;

    public WXFSInitializeOperation(WechatFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    internal override void InternalStart()
    {
        Status = EOperationStatus.Succeed;
    }
    internal override void InternalUpdate()
    {
    }
}
#endif