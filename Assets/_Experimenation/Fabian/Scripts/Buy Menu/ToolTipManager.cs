using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager _instance;

    public TextMeshProUGUI[] textComponent;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void SetAndShowToolTip()
    {
        gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        gameObject.SetActive(false);
    }


}
