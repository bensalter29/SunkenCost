﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Items
{
    public class BattleActivationShaderDisplay : MonoBehaviour
    {
        [SerializeField] private float _tweenTime = 0.3f;
        [SerializeField] private float _zoomScale = 0.75f;
        [SerializeField] private float _yOffset = -25f;

        [SerializeField] private Image _image;

        private Material _material;
        private RectTransform _rectTransform;

        private Vector3 _defaultLocalScale;
        private Vector3 _defaultLocalPosition;

        private void Awake()
        {
            var instancedMaterial = new Material(_image.material);
            _material = _image.material = instancedMaterial;
            _material.DisableKeyword("SHINE_ON");
            _rectTransform = GetComponent<RectTransform>();
            _defaultLocalScale = _rectTransform.localScale;
        }

        private void Start()
        {
            _defaultLocalPosition = _rectTransform.localPosition;
        }

        private void Tween()
        {
            LeanTween.cancel(gameObject);

            _rectTransform.localScale = _defaultLocalScale;
            _rectTransform.localPosition = _defaultLocalPosition;

            LeanTween.scale(_rectTransform, new Vector3(_zoomScale, _zoomScale, 1), _tweenTime).setLoopPingPong(1);
            LeanTween.moveLocal(gameObject, new Vector3(_defaultLocalPosition.x, _defaultLocalPosition.y + _yOffset, 1), _tweenTime).setLoopPingPong(1);
            
            
        }

        public void Activate()
        {
            Tween();
            
            StopCoroutine(Shine());
            StartCoroutine(Shine());
        }

        private IEnumerator Shine()
        {
            _material.EnableKeyword("SHINE_ON");
            var progress = 0f;

            while (progress < 1f)
            {
                _material.SetFloat("_ShineLocation", progress);
                progress += 0.05f;
                yield return new WaitForSecondsRealtime(_tweenTime / 100);
            }
            
            _material.DisableKeyword("SHINE_ON");
        }

        private IEnumerator ActivateCoroutine()
        {
            var localScale = _rectTransform.localScale;

            _material.EnableKeyword("OUTBASE_ON");
            _material.SetFloat("_OutlineAlpha", 0f);
            var progress = 0f;

            while (progress < 1f)
            {
                _material.SetFloat("_OutlineAlpha", progress);
                
            }
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            while (progress > 0)
            {
                _material.SetFloat("_OutlineAlpha", progress);
                progress -= 0.2f;
                yield return new WaitForSecondsRealtime(0.01f);
            }
            
            _material.DisableKeyword("OUTBASE_ON");
        }
    }
}