using UnityEngine;

public enum CoinColor { Red, Green, Blue, Yellow }

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("Coin Color")]
    public CoinColor coinColor;

    [Header("Visuals")]
    [SerializeField] private Renderer targetRenderer; 

    static readonly int _ColorId = Shader.PropertyToID("_Color");
    static readonly int _BaseColorId = Shader.PropertyToID("_BaseColor");

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
        gameObject.tag = "Coin";

        TryFindRenderer();
        ApplyColor();
    }

    void Awake()        { TryFindRenderer(); }
    void OnEnable()     { TryFindRenderer(); ApplyColor(); }
    void OnValidate()   { TryFindRenderer(); ApplyColor(); }

    void TryFindRenderer()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
    }

    public static Color FromCoinColor(CoinColor c)
    {
        switch (c)
        {
            case CoinColor.Red:    return Color.red;
            case CoinColor.Green:  return Color.green;
            case CoinColor.Blue:   return Color.blue;
            case CoinColor.Yellow: return Color.yellow;
            default:               return Color.white;
        }
    }

    public void ApplyColor()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[Coin] No Renderer found on '{name}'. Assign one or add a child with a Mesh Renderer.");
            return;
        }

        var col = FromCoinColor(coinColor);

        var mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(_ColorId, col);
        mpb.SetColor(_BaseColorId, col); 
        targetRenderer.SetPropertyBlock(mpb);
    }

    public void RandomizeColor()
    {
        coinColor = (CoinColor)Random.Range(0, 4);
        ApplyColor();
    }
}