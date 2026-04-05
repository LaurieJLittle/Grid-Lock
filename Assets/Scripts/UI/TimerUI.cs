using TMPro;
using UnityEngine;

namespace GridLock.UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        
        private float _timeLimit;
        
        public void SetData(float timeLimit)
        {
            _timeLimit = timeLimit;
        }

        public void UpdateTimer(float totalTimePassed)
        {
            _timerText.text = $"Time Remaining: {_timeLimit - totalTimePassed:0}";
        }

        public void Hide()
        {
            _timerText.text = string.Empty;
        }
    }
}
