using UnityEngine;

public class SampleColorSubject : MonoBehaviour
{
    [SerializeField]
    private eSampleColor _eSampleColor;

    [SerializeField]
    private bool _SendNotify;

    [SerializeField]
    private bool _SecretSendNotify;

    [SerializeField]
    private SampleImageColorChange _ImageColor;

    private SampleBoxColorChange _Box1ColorChanger;
    private SampleBoxColorChange _Box2ColorChanger;
    private SampleBoxColorChange _Box3ColorChanger;

    private Observable<eSampleColor> _ColorChangeObservable;

    private void Awake()
    {
        _Box1ColorChanger = transform.GetChild(0).GetComponent<SampleBoxColorChange>();
        _Box2ColorChanger = transform.GetChild(1).GetComponent<SampleBoxColorChange>();
        _Box3ColorChanger = transform.GetChild(2).GetComponent<SampleBoxColorChange>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ColorChangeObservable = new Observable<eSampleColor>();
        _ColorChangeObservable.RegistObserver(_Box1ColorChanger,_Box1ColorChanger.GetHashCode());
        _ColorChangeObservable.RegistObserver(_Box1ColorChanger,_Box1ColorChanger.GetHashCode());
        _ColorChangeObservable.RegistObserver(_Box2ColorChanger,_Box2ColorChanger.GetHashCode());
        _ColorChangeObservable.RegistObserver(_Box3ColorChanger, _Box3ColorChanger.GetHashCode());
        _ColorChangeObservable.RegistObserver(_ImageColor, _ImageColor.GetHashCode());
    }

    // Update is called once per frame
    void Update()
    {
        if(_SendNotify)
        {
            _SendNotify = false;
            _ColorChangeObservable.SendNotify(_eSampleColor);
        }

        if(_SecretSendNotify)
        {
            _SecretSendNotify = false;
            _ColorChangeObservable.SecretSendNotify(_eSampleColor, _ImageColor.GetHashCode());
        }
    }
}
