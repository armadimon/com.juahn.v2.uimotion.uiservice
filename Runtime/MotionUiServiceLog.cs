using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    /// <summary>
    /// 브릿지의 진단. 코어의 <c>IMotionLog</c>와 같은 이유로 접두어를 붙인다 —
    /// 콘솔에서 어디서 나온 경고인지 바로 알 수 있어야 한다.
    /// </summary>
    internal static class MotionUiServiceLog
    {
        public static void Warn(string message, Object context)
        {
            Debug.LogWarning("[UiMotion/UiService] " + message, context);
        }
    }
}
