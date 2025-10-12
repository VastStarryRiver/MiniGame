const unityNamespace = {
      unityVersion: '$UNITY_VERSION', 
      assetBundleFSEnabled: $ASSET_BUNDLEFS_ENABLED,
      assetBundleBufferCapacity: $ASSET_BUNDLE_BUFFER_CAPACITY,
      assetBundleBufferTTL: $ASSET_BUNDLE_BUFFERTTL,
      optimizeWebGLMemoryInBackground: $OPTIMIZE_WEBGL_MEMORY_INBACKGROUND,
      bootConfig: '$BOOT_CONFIG_INFO',
	  UnityModule: null,
}
GameGlobal.unityNamespace = GameGlobal.unityNamespace || unityNamespace;
module.exports = unityNamespace;