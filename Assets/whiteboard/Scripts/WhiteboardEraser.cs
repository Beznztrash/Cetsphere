using System.Linq;
using UnityEngine;

public class WhiteboardEraser : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _eraserSize = 20;
    [SerializeField, Tooltip("0 = hard erase (set to eraseColor). >0 = soft erase (blend towards eraseColor).")]
    private float _softness = 0f;
    [SerializeField, Tooltip("Target color to erase to. Commonly Color.white for a whiteboard.")]
    private Color _eraseColor = Color.white;

    private float _tipHeight;
    private RaycastHit _touch;
    private Whiteboard _whiteboard;

    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    private Color[] _eraseColors;

    private void Start()
    {
        if (_tip == null)
        {
            Debug.LogWarning("WhiteboardEraser: Tip transform is not assigned.");
        }

        _tipHeight = _tip != null ? _tip.localScale.y : 0.02f; // small default reach
        _eraseColors = Enumerable.Repeat(_eraseColor, _eraserSize * _eraserSize).ToArray();
    }

    private void Update()
    {
        Draw();
    }

    private void Draw()
    {
        if (_tip == null)
            return;

        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }

                if (_whiteboard == null || _whiteboard.texture == null)
                    return;

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_eraserSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_eraserSize / 2));

                // Clamp so our eraser block stays within texture bounds
                x = Mathf.Clamp(x, 0, (int)_whiteboard.textureSize.x - _eraserSize);
                y = Mathf.Clamp(y, 0, (int)_whiteboard.textureSize.y - _eraserSize);

                // Erase at the current position (always erase on contact)
                EraseAt(x, y);

                // If we were erasing last frame, fill the gap for continuous strokes
                if (_touchedLastFrame)
                {
                    for (float f = 0.01f; f <= 1.00f; f += 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);

                        lerpX = Mathf.Clamp(lerpX, 0, (int)_whiteboard.textureSize.x - _eraserSize);
                        lerpY = Mathf.Clamp(lerpY, 0, (int)_whiteboard.textureSize.y - _eraserSize);

                        EraseAt(lerpX, lerpY);
                    }

                    // Stabilize orientation while erasing
                    transform.rotation = _lastTouchRot;
                }

                _whiteboard.texture.Apply();

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }

    private void EraseAt(int x, int y)
    {
        if (_softness <= 0f)
        {
            // Hard erase: set block directly to eraseColor
            if (_eraseColors == null || _eraseColors.Length != _eraserSize * _eraserSize)
            {
                _eraseColors = Enumerable.Repeat(_eraseColor, _eraserSize * _eraserSize).ToArray();
            }
            _whiteboard.texture.SetPixels(x, y, _eraserSize, _eraserSize, _eraseColors);
        }
        else
        {
            // Soft erase: blend existing pixels towards eraseColor
            var existing = _whiteboard.texture.GetPixels(x, y, _eraserSize, _eraserSize);
            for (int i = 0; i < existing.Length; i++)
            {
                existing[i] = Color.Lerp(existing[i], _eraseColor, _softness);
            }
            _whiteboard.texture.SetPixels(x, y, _eraserSize, _eraserSize, existing);
        }
    }
}
