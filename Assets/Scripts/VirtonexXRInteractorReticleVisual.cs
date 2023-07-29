using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Drawing;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor helper object that draws a targeting <see cref="reticlePrefab"/> over a ray casted point in front of the Interactor.
    /// </summary>
    /// <remarks>
    /// When attached to an <see cref="XRRayInteractor"/>, the <see cref="XRRayInteractor.TryGetCurrentRaycast"/> 
    /// method will be used instead of the internal ray cast function of this behavior.
    /// </remarks>
    [AddComponentMenu("XR/Visual/XR Interactor Reticle Visual", 11)]
    [DisallowMultipleComponent]
    public class VirtonexXRInteractorReticleVisual : MonoBehaviour
    {
        const int k_MaxRaycastHits = 10;

        [SerializeField]
        float m_MaxRaycastDistance = 10f;
        /// <summary>
        /// The max distance to Raycast from this Interactor.
        /// </summary>
        public float maxRaycastDistance
        {
            get => m_MaxRaycastDistance;
            set => m_MaxRaycastDistance = value;
        }

        [SerializeField]
        GameObject m_ReticlePrefab;
        /// <summary>
        /// Prefab which Unity draws over Raycast destination.
        /// </summary>
        public GameObject reticlePrefab
        {
            get => m_ReticlePrefab;
            set
            {
                m_ReticlePrefab = value;
                SetupReticlePrefab();
            }
        }

        [SerializeField]
        float m_PrefabScalingFactor = 1f;
        /// <summary>
        /// Amount to scale prefab (before applying distance scaling).
        /// </summary>
        public float prefabScalingFactor
        {
            get => m_PrefabScalingFactor;
            set => m_PrefabScalingFactor = value;
        }

        [SerializeField]
        bool m_UndoDistanceScaling = true;
        /// <summary>
        /// Whether Unity undoes the apparent scale of the prefab by distance.
        /// </summary>
        public bool undoDistanceScaling
        {
            get => m_UndoDistanceScaling;
            set => m_UndoDistanceScaling = value;
        }

        [SerializeField]
        bool m_AlignPrefabWithSurfaceNormal = true;
        /// <summary>
        /// Whether Unity aligns the prefab to the ray casted surface normal.
        /// </summary>
        public bool alignPrefabWithSurfaceNormal
        {
            get => m_AlignPrefabWithSurfaceNormal;
            set => m_AlignPrefabWithSurfaceNormal = value;
        }

        [SerializeField]
        bool m_DrawWhileSelecting;
        /// <summary>
        /// Whether Unity draws the <see cref="reticlePrefab"/> while selecting an Interactable.
        /// </summary>
        public bool drawWhileSelecting
        {
            get => m_DrawWhileSelecting;
            set => m_DrawWhileSelecting = value;
        }

        [SerializeField]
        LayerMask m_RaycastMask = -1;
        /// <summary>
        /// Layer mask for ray cast.
        /// </summary>
        public LayerMask raycastMask
        {
            get => m_RaycastMask;
            set => m_RaycastMask = value;
        }

        bool m_ReticleActive;
        /// <summary>
        /// Whether the reticle is currently active.
        /// </summary>
        public bool reticleActive
        {
            get => m_ReticleActive;
            set
            {
                m_ReticleActive = value;
                if (m_ReticleInstance != null)
                    m_ReticleInstance.SetActive(value);
            }
        }

        GameObject m_ReticleInstance;
        XRBaseInteractor m_Interactor;
        Vector3 m_TargetEndPoint;
        Vector3 m_TargetEndNormal;
        PhysicsScene m_LocalPhysicsScene;

        private Color m_Color = new Color32(255, 255, 225, 150);
        private Color m_ColorValid = new Color32(97, 197, 217, 255);
        private Color m_ColorInvalid = new Color32(255, 255, 225, 150);

        /// <summary>
        /// Reusable array of ray cast hits.
        /// </summary>
        readonly RaycastHit[] m_RaycastHits = new RaycastHit[k_MaxRaycastHits];

        private bool grabActive = false;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_LocalPhysicsScene = gameObject.scene.GetPhysicsScene();

            m_Interactor = GetComponent<XRBaseInteractor>();
            if (m_Interactor != null)
            {
                m_Interactor.selectEntered.AddListener(OnSelectEntered);
                m_Interactor.selectExited.AddListener(OnSelectExited);
            }
            SetupReticlePrefab();
            reticleActive = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            if (!grabActive)
            {
                if (m_Interactor != null && UpdateReticleTarget())
                    ActivateReticleAtTarget();
                else
                {
                    m_Color = m_ColorInvalid;
                    ActivateReticle();
                }
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_Interactor != null)
            {
                m_Interactor.selectEntered.RemoveListener(OnSelectEntered);
                m_Interactor.selectExited.RemoveListener(OnSelectExited);
            }
        }

        protected void OnDisable()
        {
            reticleActive = false;
        }

        void SetupReticlePrefab()
        {
            if (m_ReticleInstance != null)
                Destroy(m_ReticleInstance);

            if (m_ReticlePrefab != null)
                m_ReticleInstance = Instantiate(m_ReticlePrefab);
        }

        static RaycastHit FindClosestHit(RaycastHit[] hits, int hitCount)
        {
            var index = 0;
            var distance = float.MaxValue;
            for (var i = 0; i < hitCount; ++i)
            {
                if (hits[i].distance < distance)
                {
                    distance = hits[i].distance;
                    index = i;
                }
            }

            return hits[index];
        }

        bool TryGetRaycastPoint(ref Vector3 raycastPos, ref Vector3 raycastNormal)
        {
            var raycastHit = false;

            // Raycast against physics
            var hitCount = m_LocalPhysicsScene.Raycast(m_Interactor.attachTransform.position, m_Interactor.attachTransform.forward,
                m_RaycastHits, m_MaxRaycastDistance, m_RaycastMask);
            if (hitCount != 0)
            {
                var closestHit = FindClosestHit(m_RaycastHits, hitCount);
                raycastPos = closestHit.point;
                raycastNormal = closestHit.normal;
                raycastHit = true;
            }

            return raycastHit;
        }

        bool UpdateReticleTarget()
        {
            if (!m_DrawWhileSelecting && m_Interactor.hasSelection)
                return false;

            var hasRaycastHit = false;
            var raycastPos = Vector3.zero;
            var raycastNormal = Vector3.zero;

            if (m_Interactor is XRRayInteractor rayInteractor)
            {
                if (rayInteractor.TryGetCurrentRaycast(out var raycastHit, out _, out var uiRaycastHit, out _, out var isUIHitClosest))
                {
                    if (isUIHitClosest)
                    {
                        m_Color = m_ColorValid;
                        Debug.Assert(uiRaycastHit.HasValue, this);
                        var hit = uiRaycastHit.Value;
                        raycastPos = hit.worldPosition;
                        raycastNormal = hit.worldNormal;
                        hasRaycastHit = true;
                    }
                    else if (raycastHit.HasValue)
                    {
                        m_Color = m_ColorInvalid;
                        var hit = raycastHit.Value;
                        raycastPos = hit.point;
                        raycastNormal = hit.normal;
                        hasRaycastHit = true;
                    }
                }
            }
            else if (TryGetRaycastPoint(ref raycastPos, ref raycastNormal))
            {
                m_Color = m_ColorValid;
                hasRaycastHit = true;
            }

            if (hasRaycastHit)
            {
                m_TargetEndPoint = raycastPos;
                m_TargetEndNormal = raycastNormal;
                return true;
            }
            return false;
        }

        void ActivateReticleAtTarget()
        {
            if (m_ReticleInstance != null)
            {
                m_ReticleInstance.transform.position = m_TargetEndPoint;
                if (m_AlignPrefabWithSurfaceNormal)
                    m_ReticleInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, m_TargetEndNormal);
                else
                    m_ReticleInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, (m_Interactor.attachTransform.position - m_TargetEndPoint).normalized);
                var scaleFactor = m_PrefabScalingFactor;
                if (m_UndoDistanceScaling)
                    scaleFactor *= Vector3.Distance(m_Interactor.attachTransform.position, m_TargetEndPoint) * 0.393f + 0.7f;
                m_ReticleInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                m_ReticleInstance.GetComponentInChildren<Image>().color = m_Color;

                reticleActive = true;
            }
        }

        void ActivateReticle()
        {
            if (m_ReticleInstance != null)
            {
                m_TargetEndPoint = m_Interactor.attachTransform.position + m_Interactor.attachTransform.forward * m_MaxRaycastDistance;
                m_ReticleInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, (m_Interactor.attachTransform.position - m_TargetEndPoint).normalized);

                m_ReticleInstance.transform.position = m_TargetEndPoint;

                var scaleFactor = m_PrefabScalingFactor;
                if (m_UndoDistanceScaling)
                    scaleFactor *= Vector3.Distance(m_Interactor.attachTransform.position, m_TargetEndPoint) * 0.393f + 0.7f;
                m_ReticleInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                m_ReticleInstance.GetComponentInChildren<Image>().color = m_Color;

                reticleActive = true;
            }
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            grabActive = true;
            reticleActive = false;
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            grabActive = false;
        }
    }
}
