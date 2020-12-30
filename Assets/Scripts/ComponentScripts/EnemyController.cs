﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    int dir;
    float sizeMult = 1;
    RoomEnemiesController controller;
    public bool isAlive;

    private Animator animator;
    [HideInInspector] public int currentHealth; //Salud actual del enemigo
    public int maxHealth; //Salud máxima del enemigo
    public bool isVulnerable; //Booleano que determina si el enemigo puede recibir daño en su estado actual.
    [SerializeField] float invulnerabilityAfterHitTime; //tiempo en segundos que pasa el enemigo siendo invulnerable tras recibir daño.
    //private EnemyHealthBar healthBar;

    //Variables relacionadas con el sistema de detección de enemigos en pantalla
    [HideInInspector] public bool onScreen;
    [HideInInspector] public bool addedToList;
    private float timeOutTimer;

    void Start()
    {
        isVulnerable = true;
        dir = (Random.value<0.5f)?-1:1;
        sizeMult = Random.Range(0.5f, 1.5f);
        transform.localScale = new Vector3(sizeMult, sizeMult, sizeMult);
        animator = GetComponent<Animator>();
    }

    public void setRoomEnemyController(RoomEnemiesController rEC)
    {
        controller = rEC;
    }

    public void enemyDefeated()
    {
        if(controller!= null){
            controller.enemyDefeated();
        }
        Destroy(gameObject);
    }
    void Update()
    {
        //transform.Rotate(0, dir, 0);

        //GESTIONAR SI EL ENEMIGO ESTÁ EN PANTALLA, Y SI LO ESTÁ, AÑADIRLO A LA LISTA DE ENEMIGOS EN EL COMBAT MANAGER

        //Determinamos la posición relativa del enemigo en el plano de la cámara.
        Vector3 enemyPosition = Camera.main.WorldToViewportPoint(transform.position);

        //Si los valores X e Y del vector anterior están entre 0 y 1, el enemigo se encuentra dentro de la pantalla.
        onScreen = enemyPosition.z > 0 && enemyPosition.x > 0 && enemyPosition.x < 1 && enemyPosition.y > 0 && enemyPosition.y < 1;

        //Finalmente, si el enemigo está dentro de la pantalla, lo añadimos a la lista de enemigos en el manager (una sola vez)
        if (onScreen && !addedToList)
        {
            addedToList = true;
            timeOutTimer = 0;
            CombatManager.CM.enemies.Add(this);
        }
        else if (!onScreen) //Pero si sale de la pantalla, no nos interesa mantenerlo en la lista, de manera que lo quitamos.
        {
            //Si el enemigo esta fuera de la pantalla,
            timeOutTimer += Time.deltaTime; //corre el contador de tiempo,
            if (timeOutTimer >= 4500) //si dicho contador supera los 4,5 segundos fuera de pantalla
            {
                //quita al enemigo de la lista
                addedToList = false;
                CombatManager.CM.enemies.Remove(this);
                timeOutTimer = 0;
            }
        }

        if (onScreen && addedToList)
        {

            //if (statsWidget.activeSelf == true) //Si las stats están siendo mostradas en pantalla,
            //{
            //    //Actualiza la posición del widget para que siga al enemigo,
            //    Vector3 viewportPosition = Camera.main.WorldToScreenPoint(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z));
            //    statsWidget.transform.position = new Vector3(viewportPosition.x, viewportPosition.y, -3);

            //    //Y activa la información de HP y WP
            //    statsWidget.GetComponent<StatsDisplayer>().updateWPandHP(currentHealth, willpower);
            //}
        }
    }


    public void takeDamage(int damage, float knockbackForce, Vector3 knockbackDir, GameObject other)
    {
        print("OUCH");
        if (isVulnerable) //Si el enemigo es vulnerable,
        {
            currentHealth -= damage;
            StartCoroutine(cooldownVulnerability());

            //Play hurt animation
            //animator.SetTrigger("Hurt");


            //Look at the one who attacked
            transform.LookAt(new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z));

            //Apply knockback received from the 'other' who attacks
            //if (knockbackForce != 0)
            //    forceApplier.AddImpact(new Vector3(knockbackDir.x, 0, knockbackDir.z), knockbackForce);

            if (currentHealth <= 0)
            {
                Die(); //Si el HP se reduce por debajo de 0, el enemigo muere.
            }
        }

    }

    private void Die()
    {
        //Play death animation
        //animator.SetTrigger("Death");

        isAlive = false;

        CombatManager.CM.enemies.Remove(this);

        this.enabled = false;

        
        Destroy(gameObject, 3f);
    }

    public IEnumerator cooldownVulnerability()
    {
        isVulnerable = false;
        yield return new WaitForSeconds(invulnerabilityAfterHitTime);
        isVulnerable = true;
    }
}
