using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public Material[] rightMaterials; // Materialien f�r die Bewegung nach rechts
    public Material[] leftMaterials; // Materialien f�r die Bewegung nach links
    public Material throwMaterialRight; // Wurf-Material f�r die rechte Seite
    public Material throwMaterialLeft; // Wurf-Material f�r die linke Seite
    public float moveSpeed = 2f; // Geschwindigkeit anpassen
    public float jumpForce = 5f; // St�rke des Sprungs
    public GameObject burgerPrefab; // Das Burger-Prefab, das geworfen wird
    public Transform throwPointRight; // Wurfpunkt f�r rechts
    public Transform throwPointLeft; // Wurfpunkt f�r links
    public float throwForce = 10f; // Die Wurfkraft des Burgers
    public float throwAnimationDuration = 0.5f; // Dauer der Wurf-Animation

    private Renderer rend;
    private Coroutine materialSwitchCoroutine;
    private int currentMaterialIndex = 0; // Index f�r das aktuell verwendete Material
    private Material[] currentMaterials; // Die aktuell verwendeten Materialien
    private bool isRunning = false; // Status, ob die Animation l�uft
    private bool isGrounded = true; // Status, ob der Charakter den Boden ber�hrt
    private bool facingRight = true; // Aktuelle Blickrichtung
    private bool isThrowing = false; // Status, ob gerade ein Wurf stattfindet

    void Start()
    {
        rend = GetComponent<Renderer>();
        currentMaterials = rightMaterials; // Setze die Anfangsmaterialien auf die rechten Materialien
        rend.material = currentMaterials[0]; // Setze das Anfangsmaterial
    }

    void Update()
    {
        // Bewegung auf der X- und Z-Achse abfragen
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;

        // Bestimme die Materialien basierend auf der horizontalen Bewegungsrichtung
        if (!isThrowing) // W�hrend des Werfens darf sich das Material nicht �ndern
        {
            if (moveX > 0) // Rechts
            {
                currentMaterials = rightMaterials; // Setze die aktuellen Materialien auf die rechten Materialien
                facingRight = true;
            }
            else if (moveX < 0) // Links
            {
                currentMaterials = leftMaterials; // Setze die aktuellen Materialien auf die linken Materialien
                facingRight = false;
            }
        }

        // Wenn sich das Quad bewegt
        if (movement.magnitude > 0.01f) // Laufen auch w�hrend der Wurf-Animation erlauben
        {
            // Bewege das Quad
            transform.Translate(movement * Time.deltaTime * moveSpeed); // Geschwindigkeit anpassen

            // Starte den Materialwechsel, wenn er nicht l�uft
            if (!isRunning && !isThrowing)
            {
                isRunning = true;
                StartCoroutine(SwitchMaterials(currentMaterials));
            }
        }
        else if (!isThrowing)
        {
            // Stoppe die Animation, wenn keine Bewegung mehr und kein Wurf
            if (isRunning)
            {
                isRunning = false;
                StopAllCoroutines();
                rend.material = currentMaterials[0]; // Setze das Material auf das Standardmaterial zur�ck
            }
        }

        // Sprunglogik
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Burger werfen
        if (Input.GetKeyDown(KeyCode.F) && !isThrowing)
        {
            StartCoroutine(ThrowBurger());
        }

        // Schwerkraft anwenden
        if (!isGrounded)
        {
            ApplyGravity();
        }
    }

    private IEnumerator SwitchMaterials(Material[] materials)
    {
        while (isRunning) // Wechsle nur, solange die Animation l�uft
        {
            rend.material = materials[currentMaterialIndex];
            currentMaterialIndex = 1 - currentMaterialIndex; // Wechsel zwischen 0 und 1
            yield return new WaitForSeconds(0.1f); // Zeit zwischen den Materialwechseln
        }
    }

    private void Jump()
    {
        // F�ge dem Charakter einen vertikalen Sprung hinzu
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Der Charakter ist jetzt in der Luft
        }
    }

    private void ApplyGravity()
    {
        // Schwerkraft anwenden, falls der Charakter in der Luft ist
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Physics.gravity * rb.mass);
        }
    }

    private IEnumerator ThrowBurger()
    {
        isThrowing = true; // Blockiere den Materialwechsel, aber nicht die Bewegung w�hrend des Werfens

        // Setze das Wurfmaterial f�r die Wurfanimation
        rend.material = facingRight ? throwMaterialRight : throwMaterialLeft;

        // Bestimme den Wurfpunkt basierend auf der Blickrichtung
        Transform throwPoint = facingRight ? throwPointRight : throwPointLeft;

        // Erstelle eine Kopie des Burgers an der gew�hlten Wurfposition
        GameObject thrownBurger = Instantiate(burgerPrefab, throwPoint.position, throwPoint.rotation);

        // F�ge dem Burger einen Rigidbody hinzu, falls er noch keinen hat
        Rigidbody rb = thrownBurger.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownBurger.AddComponent<Rigidbody>();
        }

        // Wurfrichtung basierend auf der Blickrichtung
        Vector3 throwDirection = facingRight ? Vector3.right : Vector3.left;

        // Wende Kraft auf den Burger an
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Warte f�r die Dauer der Wurfanimation
        yield return new WaitForSeconds(throwAnimationDuration);

        // Nach der Wurfanimation, setze die Bewegung und Animation zur�ck
        rend.material = currentMaterials[0]; // Zur�ck zu den laufenden Materialien
        isThrowing = false; // Wurf ist vorbei, Animation und Bewegung sind wieder erlaubt
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �berpr�fen, ob der Charakter den Boden ber�hrt
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Der Charakter ist wieder auf dem Boden
        }
    }
}
