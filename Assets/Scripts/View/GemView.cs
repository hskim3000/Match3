﻿using System;
using DG.Tweening;
using UnityEngine;

public interface IGemView
{
    int Endurance { get; }
    Position Position { get; }
    GemModel id { get; }
    Int64 Deadline { get; }
    Int64 PreservedUntil { get; }
    GemModel UpdateModel(GemModel gemModel);
}

public class GemView: BaseView<GemModel, GemController<GemModel>>
{
    private SpriteRenderer spriteRenderer;
    private TextMesh idText;
    private TextMesh markerIdText;
    
    void Awake()
    {
        idText = ResourceCache.Instantiate("ID", transform).GetComponent<TextMesh>();
        markerIdText = ResourceCache.Instantiate("MarkerID", transform).GetComponent<TextMesh>();
        markerIdText.gameObject.SetActive(false);
    }
    
    void OnEnable() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public Position Position 
    { 
        get { return Model.Position; } 
    }

    public Int64 ID 
    { 
        get { return Model.id; }
    }

    public Int64 PreservedFromMatch
    {
        get { return Model.preservedFromMatch; }
    }

    bool showID = true;

    public void UpdateModel(GemModel gemModel) 
    {
        Model = gemModel;
        if (showID) 
        {
            idText.text = gemModel.id.ToString();
        }
    }
    
    public void Highlight() 
    {
        var duration = 0.395f;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);

        DOTween.To(
            () => mpb.GetFloat("_FlashAmount"),
            value => {
                mpb.SetFloat("_FlashAmount", value);
                spriteRenderer.SetPropertyBlock(mpb);
            }, 
            .2f, 
            duration
        ).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void Squash() 
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(new Vector3(1.08f, 0.92f, 1), 0.12f));
        sequence.Append(transform.DOScale(new Vector3(1, 1, 1), 0.68f).SetEase(Ease.OutElastic));

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.0f);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    public void Reveal() 
    {
        gameObject.SetActive(true);

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.4f);
        spriteRenderer.SetPropertyBlock(mpb);

        var color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void Hide()
    {
        var color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r, color.g, color.b, 0.1f);
    }

    public void SetBlock(Int64 markerID) 
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.4f);
        mpb.SetColor("_FlashColor", new Color32(255, 0, 0, 1));
        spriteRenderer.SetPropertyBlock(mpb);

        markerIdText.text = markerID.ToString();
        markerIdText.gameObject.SetActive(true);
    }

    public void SetActive(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetLocalPosition(Vector2 position)
    {
        transform.localPosition = position;
    }

    public override void ReturnToPool()
    {   
        markerIdText.gameObject.SetActive(false);
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.0f);
        spriteRenderer.SetPropertyBlock(mpb);

        base.ReturnToPool();
    }
}
