using Unity.Burst.CompilerServices;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Tooltip("Сила отскока (вертикальная скорость)")]
    public float bounceForce = 15f;

    // Вызывается при столкновении CharacterController с этим коллайдером
   
    private void OnTriggerEnter(Collider hit)
    {// Проверяем, что столкновение произошло с игроком (по тегу)
       // if (hit.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = hit.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {

                player.Bounce(bounceForce);

            }
        }

    }
}