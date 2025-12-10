using TMPro;
using UnityEngine;

public class LightRotator : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] float speed;
    [SerializeField] Vector3 rotation;

    public void SetSpeed(float value)
    {
        speed = Mathf.Clamp(value, -25f, 25f);
    }

    void FixedUpdate()
    {
        Vector3 newAngles = transform.rotation.eulerAngles + rotation * speed * Time.deltaTime;
        transform.eulerAngles = newAngles;
        infoText.text = speed.ToString();
        if(Input.GetKeyDown(KeyCode.R))
        {
            speed = 25;
        }
        else if(Input.GetKeyDown(KeyCode.Q))
        {
            speed =-25;
        }
        speed = Mathf.Clamp(speed, -25f, 25f);
    }
}
