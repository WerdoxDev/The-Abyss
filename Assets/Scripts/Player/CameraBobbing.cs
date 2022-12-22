using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float bobbingSpeed = 14f;

    private float _timer = 0f;
    private float _defaultY = 0f;

    private void Start()
    {
        _defaultY = transform.localEulerAngles.x;
    }

    private void Update()
    {
        _timer += Time.deltaTime * bobbingSpeed;
        transform.localEulerAngles = new Vector3(_defaultY + Mathf.Sin(_timer) * bobbingAmount, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
}
