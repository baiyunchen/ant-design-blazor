﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using AntBlazor.Internal;
using AntBlazor.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AntBlazor
{
    public partial class Dropdown : OverlayTrigger
    {
        [Parameter]
        public Func<RenderFragment, RenderFragment, RenderFragment> ButtonsRender { get; set; }

        private string _rightButtonIcon = "ellipsis";
        private string _buttonSize = AntSizeLDSType.Default;
        private string _buttonType = AntButtonType.Default;

        private RenderFragment _leftButton;
        private RenderFragment _rightButton;

        [Inject]
        private DomEventService DomEventService { get; set; }

        protected void ChangeRightButtonIcon(string icon)
        {
            _rightButtonIcon = icon;

            StateHasChanged();
        }

        protected void ChangeButtonSize(string size)
        {
            _buttonSize = size;

            StateHasChanged();
        }

        protected void ChangeButtonType(string type)
        {
            _buttonType = type;

            StateHasChanged();
        }
    }
}
