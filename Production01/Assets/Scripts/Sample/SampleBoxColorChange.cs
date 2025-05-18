using UnityEngine;

public enum eSampleColor
{
    Red,
    Blue,
    Yellow,
    White,
}

public class SampleBoxColorChange : MonoBehaviour,IObserver<eSampleColor>
{
    private MeshRenderer _MeshRenderer;

    void Awake()
    {
        _MeshRenderer = GetComponent<MeshRenderer>();
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

        _MeshRenderer.material.color = color;
    }
}
