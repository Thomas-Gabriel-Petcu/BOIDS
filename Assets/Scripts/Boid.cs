using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Boid : MonoBehaviour
{
    #region Fields
    private Rigidbody2D _rb;

    [SerializeField]
    private float _speed;

    [Range(0, 360)]
    [SerializeField]
    private float _viewAngle;

    [SerializeField]
    private float _viewRadius;

    [SerializeField]
    private LayerMask _boidMask;

    [SerializeField]
    private float rotateSpeed;

    private List<Collider2D> _boidsInView;
    private float _boidsInViewAvgRotation;

    private bool _CoroutineRunning = false;
    private Quaternion _desiredRotation;
    private List<Collider2D> _collList;
    private Vector3 _centerOfBoidsInView;
    #endregion

    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
    }
    //start is called before the first frame update
    void Start()
    {
        _collList = new List<Collider2D>();
        //_rb.velocity = (transform.right * _speed) * Time.deltaTime;
    }
    private void Update()
    {
        //Debug.Log(_CoroutineRunning);

        _boidsInView = GetBoidsInView();
        if (_boidsInView.Count == 0) return;

        if (_CoroutineRunning == false) StartCoroutine(AlignToBoidsInView(_rb, _boidsInView));

    }
    private void FixedUpdate()
    {
        _rb.velocity = (transform.right * _speed) * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Vector3 l_v1 = DirFromAngle(_viewAngle / 2, false);
        Vector3 l_v2 = DirFromAngle(-_viewAngle / 2, false);
    
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + l_v1 * _viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + l_v2 * _viewRadius);

    }

    public Vector2 DirFromAngle(float angleInDeg, bool isGlobalAngle)
    {
        if (!isGlobalAngle)
        {
            angleInDeg += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Sin(angleInDeg * Mathf.Deg2Rad + (90 * Mathf.Deg2Rad)), -Mathf.Cos(angleInDeg * Mathf.Deg2Rad + (90 * Mathf.Deg2Rad)));
    }

    private List<Collider2D> GetBoidsInView()
    {
        //Checks for all boids in view radius, excluding itself
        Collider2D[] l_colliders = Physics2D.OverlapCircleAll(transform.position, _viewRadius, _boidMask);

        _collList.Clear();
        foreach (Collider2D l_col in l_colliders)
        {
            Vector3 l_dirToCol = (l_col.transform.position - transform.position).normalized;
            if (!(Vector3.Angle(transform.right, l_dirToCol) < _viewAngle / 2)) break;
            _collList.Add(l_col);
        }
        _collList.Remove(GetComponent<Collider2D>());
        return _collList;
        //collList now only contains other boids' colliders, excluding this one
    }

    private IEnumerator AlignToBoidsInView(Rigidbody2D l_rb, List<Collider2D> l_boidColliders)
    {
        _CoroutineRunning = true;
        _boidsInViewAvgRotation = 0;
        foreach (Collider2D l_boidCol in l_boidColliders)
        {
            _boidsInViewAvgRotation += l_boidCol.transform.rotation.z;
        }
        _boidsInViewAvgRotation /= l_boidColliders.Count;
        _desiredRotation = new Quaternion(transform.rotation.x, transform.rotation.y, _boidsInViewAvgRotation, transform.rotation.w).normalized;

        do
        {
            l_rb.transform.rotation = Quaternion.Slerp(transform.rotation, _desiredRotation, Time.deltaTime * rotateSpeed);
            yield return null;
        } while (Quaternion.Angle(transform.rotation,_desiredRotation) > 0.01f);
        _CoroutineRunning = false;
        yield break;
    }

    //private Vector3 FindCenterOfBoids(List<Collider2D> l_boidColliders)
    //{

    //}

}
