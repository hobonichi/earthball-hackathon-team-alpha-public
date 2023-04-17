// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;
using UnityEngine.EventSystems;
using Hobonichi;
// using System.Collections.Generic;
using System.Linq;

public class SampleSphere : MonoBehaviour
{
  [SerializeField] GameObject currentTarget;
  [SerializeField] Material[] materials;
  [SerializeField] GameObject[] prefabs;
  [SerializeField] AudioClip[] audioClips;
  public float startDirection;

  GameObject prefab_instance = null;
  int pageId = -1;
  bool targetFound = false;

  void Start()
  {
    if (currentTarget)
    {
      currentTarget.GetComponent<Collider>().enabled = false;
    }
  }

  public void SetDragTarget(GameObject target)
  {
    if (currentTarget)
    {
      currentTarget.GetComponent<Collider>().enabled = true;
    }

    currentTarget = target;
    currentTarget.GetComponent<Collider>().enabled = false;
  }

  public void OnTapDown(BaseEventData args)
  {
    if (currentTarget)
    {
      var p = args as PointerEventData;
      var loc = SphericalGeometry.Pos2LLH(transform.InverseTransformPoint(p.pointerCurrentRaycast.worldPosition));
      loc.Set(loc.x, loc.y, 1);
      currentTarget.GetComponent<ObjectOnSphere>().location = loc;
    }
  }

  void Update()
  {
    if (targetFound)
    {
      var pageNumber = new[] { materials.Length, prefabs.Length, audioClips.Length }.Max();
      var pageAngleStep = 360f / pageNumber;

      // 地球のオブジェクトから、ARCamera のオブジェクトへの、方向ベクトル
      var direction = currentTarget.transform.position - Camera.main.transform.position;
      // // 同じ向きを向くように回転させたい場合には、Quaternion.LookRotation 関数が使えるようです
      // var rotation = Quaternion.LookRotation(direction);

      // ここから、例えば地軸を軸としてカメラの方向を知りたいなら…
      // まず、このベクトルを地球儀のローカル座標系に変換します。これは、地球儀の回転の逆数を適用することで行えます。
      var directionEarthLocal = Quaternion.Inverse(currentTarget.transform.rotation) * direction.normalized;

      // ベクトルのxとz成分からカメラが向いている経度を計算します。これは、Atan2関数を使用して計算できます。ただし、Unityでは、z軸が前方を表すため、z成分をx成分と入れ替える必要があります。
      var longitudeOrig = Mathf.Atan2(directionEarthLocal.z, directionEarthLocal.x) * Mathf.Rad2Deg;
      var longitude = startDirection - longitudeOrig + pageAngleStep / 2;
      while (longitude < 0) longitude += 360f;
      while (longitude > 360) longitude -= 360f;

      // Debug.Log(direction);
      // Debug.Log(rotation);
      // Debug.Log(directionEarthLocal);
      // Debug.Log(longitude);

      var newPageId = (int)(longitude / pageAngleStep);
      if (pageId != newPageId)
      {
        Debug.Log($"orig: {longitudeOrig:F2}, longitude: {longitude:F2}, page: {newPageId}");

        // 背景変更
        var materialId = newPageId % materials.Length;
        GetComponent<Renderer>().material = materials[materialId];
        Debug.Log($"Set prefab: {materials[materialId].name}");

        // プレハブ呼び出し
        if (prefab_instance) Destroy(prefab_instance);
        var prefabId = newPageId % prefabs.Length;
        if (prefabs[prefabId])
        {
          prefab_instance = Instantiate(prefabs[prefabId], GetComponent<Transform>());
          Debug.Log($"Set prefab: {prefabs[prefabId].name}");
        }

        // 効果音再生
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource.isPlaying) audioSource.Stop();
        var audioClipId = newPageId % audioClips.Length;
        if (audioClips[audioClipId])
        {
          audioSource.clip = audioClips[audioClipId];
          Debug.Log($"Play audio clip: {audioSource.clip.name}");
          audioSource.Play();
        }

        pageId = newPageId;
      }
    }
  }

  public void OnTargetFound()
  {
    Debug.Log("Target Found");
    targetFound = true;
  }

  public void OnTargetLost()
  {
    Debug.Log("Target Lost");
    targetFound = false;
  }
}
