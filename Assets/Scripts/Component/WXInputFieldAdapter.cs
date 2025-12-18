using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_WEBGL
using WeChatWASM;
#endif



public class WXInputFieldAdapter : MonoBehaviour, IPointerClickHandler
{
	private TMP_InputField _unityInputField;
	private bool _isKeyboardShowing = false;

	void Start()
	{
		// 获取Unity的InputField组件
		_unityInputField = GetComponent<TMP_InputField>();
	}

	// 当点击输入框时触发
	public void OnPointerClick(PointerEventData eventData)
	{
		ShowWeChatKeyboard();
	}

	private void ShowWeChatKeyboard()
	{
#if !UNITY_EDITOR && UNITY_WEBGL
		// 仅在WebGL平台（即微信小游戏环境）下执行
		if (!_isKeyboardShowing)
		{
			// 调用微信API显示键盘
			WX.ShowKeyboard(new ShowKeyboardOption 
			{
				defaultValue = _unityInputField.text, // 设置初始文本
				maxLength = _unityInputField.characterLimit > 0 ? _unityInputField.characterLimit : 140, // 设置最大长度
				confirmType = "done" // 键盘确认键样式
			});

			// 监听键盘输入、确认和完成事件
			WX.OnKeyboardInput(OnKeyboardInput);
			WX.OnKeyboardConfirm(OnKeyboardConfirm);
			WX.OnKeyboardComplete(OnKeyboardComplete);

			_isKeyboardShowing = true;
		}
#endif
	}

	// 接收输入事件
	private void OnKeyboardInput(OnKeyboardInputListenerResult result)
	{
		_unityInputField.text = result.value; // 实时更新输入框文本
	}

	// 点击键盘确认键
	private void OnKeyboardConfirm(OnKeyboardInputListenerResult result)
	{
		_unityInputField.text = result.value;
		HideWeChatKeyboard();
	}

	// 键盘收起
	private void OnKeyboardComplete(OnKeyboardInputListenerResult result)
	{
		HideWeChatKeyboard();
	}

	private void HideWeChatKeyboard()
	{
#if !UNITY_EDITOR && UNITY_WEBGL
		if (_isKeyboardShowing)
		{
			WX.HideKeyboard(new HideKeyboardOption() { success = null});
			// 移除事件监听
			WX.OffKeyboardInput(OnKeyboardInput);
			WX.OffKeyboardConfirm(OnKeyboardConfirm);
			WX.OffKeyboardComplete(OnKeyboardComplete);
			_isKeyboardShowing = false;
		}
#endif
	}
}