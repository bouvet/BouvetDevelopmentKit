using Bouvet.DevelopmentKit.Input;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools.Hands
{
    public class GenerateBoundingBox : MonoBehaviour
    {
        public bool frame;
        public bool box;
        protected Bounds bounds;
        protected MeshFilter mesh;
        protected UIManager manager;

        private void Start()
        {
            manager = UIManager.Instance;
            SetupBounds();
        }
        protected void SetupBounds()
        {
            mesh = GetComponent<MeshFilter>();

            if (mesh == null)
            {
                mesh = GetComponentInChildren<MeshFilter>();
            }

            if (mesh != null && (frame ^ box))
            {
                Quaternion initialRotation = transform.rotation;
                transform.rotation = Quaternion.identity;

                bounds = mesh.sharedMesh.bounds;
                GameObject boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boundingBox.transform.parent = mesh.transform;
                if (frame)
                    boundingBox.GetComponent<MeshRenderer>().material = manager.BoundingBoxFrameMaterial;
                if(box)
                    boundingBox.GetComponent<MeshRenderer>().material = manager.BoundingBoxMaterial;
                boundingBox.transform.localScale = bounds.size * 1.001f;
                boundingBox.transform.localPosition = bounds.center;
                boundingBox.transform.rotation = mesh.transform.rotation;

                transform.rotation = initialRotation;
                Destroy(this);
            }
        }
    }
}