using UnityEngine;
using System.Collections;
using TMPro;
using Assets.Scripts;

namespace LightsaberNamespace
{
    public class Lightsaber : MonoBehaviour
    {
        // The number of vertices to create per frame
        private const int NUM_VERTICES = 12;

        [SerializeField]
        [Tooltip("The blade object")]
        private GameObject _blade = null;

        [SerializeField]
        [Tooltip("The empty game object located at the tip of the blade")]
        private GameObject _tip = null;

        [SerializeField]
        [Tooltip("The empty game object located at the base of the blade")]
        private GameObject _base = null;

        [SerializeField]
        [Tooltip("The mesh object with the mesh filter and mesh renderer")]
        private GameObject _meshParent = null;

        [SerializeField]
        [Tooltip("The number of frames that the trail should be rendered for")]
        private int _trailFrameLength = 3;

        [SerializeField]
        [ColorUsage(true, true)]
        [Tooltip("The colour of the blade and trail")]
        private Color _colour = Color.red;

        [SerializeField]
        [Tooltip("The amount of force applied to each side of a slice")]
        private float _forceAppliedToCut = 3f;

        [SerializeField]
        [Tooltip("Dropdown to select the target object")]
        private TMP_Dropdown _targetDropdown = null;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private int[] _triangles;
        private int _frameCount;
        private Vector3 _previousTipPosition;
        private Vector3 _previousBasePosition;
        private Vector3 _triggerEnterTipPosition;
        private Vector3 _triggerEnterBasePosition;
        private Vector3 _triggerExitTipPosition;
        private Vector3 _targetPlaneNormal = Vector3.up; // Initialize with default value
        private Coroutine _rotationCoroutine;


        void Start()
        {
            // Init mesh and triangles
            _meshParent.transform.position = Vector3.zero;
            _mesh = new Mesh();
            _meshParent.GetComponent<MeshFilter>().mesh = _mesh;

            Material trailMaterial = Instantiate(_meshParent.GetComponent<MeshRenderer>().sharedMaterial);
            trailMaterial.SetColor("_Color", _colour);
            _meshParent.GetComponent<MeshRenderer>().sharedMaterial = trailMaterial;

            Material bladeMaterial = Instantiate(_blade.GetComponent<MeshRenderer>().sharedMaterial);
            bladeMaterial.SetColor("_Color", _colour);
            _blade.GetComponent<MeshRenderer>().sharedMaterial = bladeMaterial;

            _vertices = new Vector3[_trailFrameLength * NUM_VERTICES];
            _triangles = new int[_vertices.Length];

            // Set starting position for tip and base
            _previousTipPosition = _tip.transform.position;
            _previousBasePosition = _base.transform.position;

            // Add listener to dropdown
            _targetDropdown.onValueChanged.AddListener(delegate { UpdateTargetObject(); });
        }

        void LateUpdate()
        {
            // Reset the frame count once we reach the frame length
            if (_frameCount == (_trailFrameLength * NUM_VERTICES))
            {
                _frameCount = 0;
            }

            // Draw first triangle vertices for back and front
            _vertices[_frameCount] = _base.transform.position;
            _vertices[_frameCount + 1] = _tip.transform.position;
            _vertices[_frameCount + 2] = _previousTipPosition;
            _vertices[_frameCount + 3] = _base.transform.position;
            _vertices[_frameCount + 4] = _previousTipPosition;
            _vertices[_frameCount + 5] = _tip.transform.position;

            // Draw fill in triangle vertices
            _vertices[_frameCount + 6] = _previousTipPosition;
            _vertices[_frameCount + 7] = _base.transform.position;
            _vertices[_frameCount + 8] = _previousBasePosition;
            _vertices[_frameCount + 9] = _previousTipPosition;
            _vertices[_frameCount + 10] = _previousBasePosition;
            _vertices[_frameCount + 11] = _base.transform.position;

            // Set triangles
            for (int i = 0; i < NUM_VERTICES; i++)
            {
                _triangles[_frameCount + i] = _frameCount + i;
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            // Track the previous base and tip positions for the next frame
            _previousTipPosition = _tip.transform.position;
            _previousBasePosition = _base.transform.position;
            _frameCount += NUM_VERTICES;
        }

        private void UpdateTargetObject()
        {
            string selectedObjectName = _targetDropdown.options[_targetDropdown.value].text;
            GameObject selectedObject = GameObject.Find(selectedObjectName);

            if (selectedObject != null)
            {
                // Determine the plane's normal based on the selected object's transform
                _targetPlaneNormal = selectedObject.transform.up;

                // Find the child object within the selected object
                Transform child = selectedObject.transform.Find("positionLS");

                if (child != null)
                {
                    // Get the position and rotation of the child object
                    Vector3 objectPosition = child.position;
                    Quaternion objectRotation = child.rotation;

                    // Place the lightsaber at the position of the child object
                    transform.position = objectPosition;

                    // Match the lightsaber's rotation with the selected object's rotation
                    transform.rotation = objectRotation;

                    // Cancel any ongoing rotation coroutine
                    if (_rotationCoroutine != null)
                        StopCoroutine(_rotationCoroutine);

                    // Update the rotation coroutine to reflect the new rotation
                    _rotationCoroutine = StartCoroutine(RotateLightsaber());
                }
                else
                {
                    Debug.LogError("Child object not found in selected object.");
                }
            }
        }

        private IEnumerator RotateLightsaber()
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 360); // Rotate a full 360 degrees
            float duration = 1f; // Duration for the rotation (adjust as needed)
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float rotationAmount = 360f * (Time.deltaTime / duration);
                transform.Rotate(0, 0, rotationAmount, Space.Self); // Rotate around local Y-axis
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the lightsaber rotation reaches exactly the target rotation
            transform.rotation = targetRotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            _triggerEnterTipPosition = _tip.transform.position;
            _triggerEnterBasePosition = _base.transform.position;
        }

        private void OnTriggerExit(Collider other)
        {
            _triggerExitTipPosition = _tip.transform.position;

            // Create a triangle between the tip and base so that we can get the normal
            Vector3 side1 = _triggerExitTipPosition - _triggerEnterTipPosition;
            Vector3 side2 = _triggerExitTipPosition - _triggerEnterBasePosition;

            // Get the point perpendicular to the triangle above which is the normal
            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            // Transform the normal so that it is aligned with the object we are slicing's transform.
            Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

            // Get the enter position relative to the object we're cutting's local transform
            Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(_triggerEnterTipPosition);

            Plane plane = new Plane();

            plane.SetNormalAndPosition(transformedNormal, transformedStartingPoint);

            var direction = Vector3.Dot(Vector3.up, transformedNormal);

            // Flip the plane so that we always know which side the positive mesh is on
            if (direction < 0)
            {
                plane = plane.flipped;
            }

            GameObject[] intersectionObjects = Slicer.Slice(plane, other.gameObject);
            int index = 0;
            int numIntersectionObjects = intersectionObjects.Length;

            foreach (GameObject intersectionObject in intersectionObjects)
            {
                Rigidbody rigidbody = intersectionObject.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = intersectionObject.AddComponent<Rigidbody>();
                }

                // Set gravity to false
                rigidbody.useGravity = false;

                // Set kinematic
                rigidbody.isKinematic = true;

                // Uncheck automatic tensor and center of mass
                rigidbody.interpolation = RigidbodyInterpolation.None;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.centerOfMass = Vector3.zero;
                rigidbody.inertiaTensorRotation = Quaternion.identity;
                rigidbody.inertiaTensor = Vector3.one;

                rigidbody.ResetCenterOfMass();
                rigidbody.ResetInertiaTensor();

                Vector3 newNormal = transformedNormal + Vector3.up * _forceAppliedToCut;
                rigidbody.AddForce(newNormal, ForceMode.Impulse);

                intersectionObject.name = "IntersectionObject" + (index + 1);
                index++;
            }

            for (int i = numIntersectionObjects - 2; i < numIntersectionObjects; i++)
            {
                if (i >= 0)
                {
                    Destroy(intersectionObjects[i]);
                }
            }
            // Find all objects named "IntersectionObject" with a number suffix
            GameObject[] allIntersectionObjects = GameObject.FindGameObjectsWithTag("IntersectionObject");

            // Calculate the average position of all found intersection objects
            Vector3 averagePosition = Vector3.zero;
            foreach (GameObject intersectionObject in allIntersectionObjects)
            {
                averagePosition += intersectionObject.transform.position;
            }
            if (allIntersectionObjects.Length > 0)
            {
                averagePosition /= allIntersectionObjects.Length;
            }

            // Create a point at the average position
            GameObject averagePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            averagePoint.transform.position = averagePosition;
            averagePoint.name = "AveragePoint";
            averagePoint.tag = "AveragePoint";
        }
    
    }
}