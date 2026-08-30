using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabNavegation : MonoBehaviour
{
    [SerializeField] private List<Selectable> _Selectables;
    [SerializeField] private int _currentIndex = 0;
    [SerializeField] private int _maxIndex = 0;
    [SerializeField] private bool _isSelected = false;
    private void Start()
    {
        _maxIndex = _Selectables.Count - 1;
    }

    // Update is called once per frame
    private void Update()
    {
        if(Keyboard.current.tabKey.wasPressedThisFrame)
        {
            _isSelected = false;
            foreach (var selectable in _Selectables)
            {
                if(EventSystem.current.currentSelectedGameObject == selectable.gameObject)
                {
                    _currentIndex = _Selectables.IndexOf(selectable);
                    _isSelected = true;
                    break;
                }
            }
            if(!_isSelected)
            {
                return;
            }
            if(Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                Previous();
            }
            else
            {
                Next();
            }
        }
    }
    private void Next()
    {
        if (_currentIndex >= _maxIndex){
            _currentIndex = _maxIndex;
            return;
        }
        _currentIndex++;
        _Selectables[_currentIndex].Select();
    }
    private void Previous()
    {
        if (_currentIndex <= 0){
            _currentIndex = 0;
            return;
        }
        _currentIndex--;
        _Selectables[_currentIndex].Select();
    }
}
