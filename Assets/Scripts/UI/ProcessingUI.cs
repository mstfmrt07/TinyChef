using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class ProcessingUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image progressBar;
        public Color processingColor = Color.yellow;
        public Color completedColor = Color.green;

        private BaseCounter counter;
        private Camera mainCamera;

        public void Initialize(BaseCounter counter)
        {
            this.counter = counter;
            mainCamera = Camera.main;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void UpdateProgress(float progress)
        {
            if (progressBar == null) return;

            progressBar.fillAmount = Mathf.Clamp01(progress);

            // Change color based on progress
            if (progress >= 1f)
            {
                progressBar.color = completedColor;
            }
            else
            {
                progressBar.color = processingColor;
            }
        }

        private void Update()
        {
            FaceCamera();

            if (counter != null && counter.isProcessing)
            {
                UpdateProgress(counter.processProgress);
            }
        }

        private void FaceCamera()
        {
            if (mainCamera != null)
            {
                // Make UI face the camera
                Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                if (directionToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
        }
    }
}

