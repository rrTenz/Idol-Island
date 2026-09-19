using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//https://www.youtube.com/watch?v=1J_F08meUYM
public class JoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{

    public Transform player;
    Vector3 move;
    public float moveSpeed;

    public RectTransform pad;
    bool walking;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        transform.localPosition = Vector2.ClampMagnitude(eventData.position - (Vector2)pad.position, pad.rect.width * 0.5f);


        move = new Vector3(transform.localPosition.x, 0, transform.localPosition.y).normalized;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localPosition = Vector3.zero;
        move = Vector3.zero;
        StopCoroutine("PlayerMove");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine("PlayerMove");
    }

    IEnumerator PlayerMove()
    {
        while (true)
        {
            player.Translate(move * moveSpeed * Time.deltaTime, Space.World);

            if (move != Vector3.zero)   //turn player
            {
                player.rotation = Quaternion.Slerp(player.rotation, Quaternion.LookRotation(move), 5 * Time.deltaTime);
            }

            yield return null;
        }
    } 
}
