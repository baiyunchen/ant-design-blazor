﻿using Microsoft.AspNetCore.Components;

namespace AntBlazor.Docs.Shared
{
    public partial class MockBrowser
    {
        private ClassMapper ClassMapper { get; set; } = new ClassMapper();

        [Parameter]
        public int Height { get; set; }

        [Parameter]
        public bool WithUrl { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void OnInitialized()
        {
            ClassMapper.Add("browser-mockup")
                .If("with-url", () => WithUrl);
        }
    }
}
