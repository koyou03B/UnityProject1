using UnityEngine;

public class SampleImageColorChange : MonoBehaviour,IObserver<eSampleColor>
{
    private UnityEngine.UI.Image _Image;

    void Awake()
    {
        _Image = GetComponent<UnityEngine.UI.Image>();
    }

    public void OnError(System.Exception error)
    {
        throw new System.NotImplementedException();
    }

    public void OnNotify(eSampleColor state)
    {
        Color color = state switch
        {
            eSampleColor.Red => Color.red,
            eSampleColor.Blue => Color.blue,
            eSampleColor.Yellow => Color.yellow,
            eSampleColor.White => Color.white,
            _ => Color.black
        };

        _Image.color = color;
    }
}
