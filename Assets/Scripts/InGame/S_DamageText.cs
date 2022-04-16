using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_DamageText : MonoBehaviour
{
    public static S_DamageText Create(Vector3 position, int damageAmount)
    {
        Transform damageTextTransform = Instantiate(S_GameAssets.i.pfDamagePopup, position, Quaternion.identity);

        S_DamageText dmgText = damageTextTransform.GetComponent<S_DamageText>();
        dmgText.Setup(damageAmount);

        return dmgText;
    }

    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Camera playerCamera;

    private float moveYSpeed;

    private void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
        playerCamera = Camera.main;
    }
    
    public void Setup(int damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
        textColor = textMesh.color;
        disappearTimer = 1f;
        moveYSpeed = 20f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward, playerCamera.transform.rotation * Vector3.up);

        float moveDropSpeed = 25f;

        transform.position += new Vector3(0, moveYSpeed, 0) * Time.deltaTime;

        if (moveYSpeed > 0)
        {
            moveYSpeed -= moveDropSpeed * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if(disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed* Time.deltaTime;
            textMesh.color = textColor;
            if(textColor.a <0)
            {
                Destroy(gameObject);
            }
        }
    }
}
