﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecipeItemHelper : ItemPanelHelper, IPointerClickHandler
{
    private Action<int> _onClickEvent;

    public Action<int> OnClickEvent { get => _onClickEvent; protected set => _onClickEvent = value; }

    public void OnPointerClick(PointerEventData eventData)
    {
        int id = gameObject.GetInstanceID();
        _onClickEvent?.Invoke(id);
    }
}
