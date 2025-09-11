using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;

public class AnimaciónCamino : MonoBehaviour
{
    public float duration = 2f;
    public Transform destino;
    
    private bool llegadoaldestino = false;
    private TweenerCore<Vector3, Path, PathOptions> animacion;
    private bool empiezamovimiento = false;
    private Vector3[] waypoints; // Define puntos clave (incluyendo la esquina)
    

    // ReSharper disable Unity.PerformanceAnalysis
    public void PlayAnimation()
    {
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(200, 10);

        CreateWaypoints();
        
        // Crea una animación a lo largo del camino
        animacion = transform.DOPath(waypoints, duration, PathType.Linear)
            .SetEase(Ease.InOutSine)  // Suaviza el movimiento
            .OnWaypointChange(OnWaypoint)
            .SetLookAt(0.01f)
            .OnComplete((() => {empiezamovimiento = false;}));
    }
    void OnWaypoint(int index)
    {

        if (index == 2 && !llegadoaldestino)
        {
            llegadoaldestino = true;
            transform.rotation *= Quaternion.Euler(0,180,0);
            Quaternion rotacion = transform.rotation;
            animacion.Pause();  // Pausa la animación
            transform.DORotateQuaternion(rotacion * Quaternion.Euler(0,180,0),4f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>  // Cuando la rotación haya terminado
                {
                    animacion.SetLookAt(0.01f);  // Reanuda la rotación para que apunte en la dirección del movimiento
                    animacion.Play();  // Reanuda la animación
                });
        } else if (llegadoaldestino)
        {
            llegadoaldestino = false;
        }
    }
    void CreateWaypoints()
    {
        Vector3 tamañoObjeto = GetComponentInChildren<Renderer>().bounds.size;
        Vector3 tamañoDestino = destino.GetComponentInChildren<Renderer>().bounds.size;
        waypoints = new Vector3[5];
        waypoints[0] = transform.position;
        waypoints[1] = transform.position + transform.forward * Mathf.Abs(destino.position.x-transform.position.x); 
        waypoints[2] = waypoints[1] + transform.right * -(Mathf.Abs(destino.position.z-transform.position.z-tamañoObjeto.x/2-tamañoDestino.z/2)); 
        waypoints[3] =  waypoints[2]+ transform.right * (Mathf.Abs(destino.position.z-transform.position.z-tamañoObjeto.x/2-tamañoDestino.z/2));
        waypoints[4] = transform.position ;
        ImprimirWaypoints();
    }
    
    void ImprimirWaypoints()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            Debug.Log("Waypoint " + i + ": " + waypoints[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //KeyMap();
    }

    private void KeyMap()
    {
        if (Input.GetKey(KeyCode.L) && !empiezamovimiento)
        {
            empiezamovimiento = true;
            PlayAnimation();
        }
    }
}
