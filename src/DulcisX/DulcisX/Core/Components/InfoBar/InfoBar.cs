﻿using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace DulcisX.Core.Components
{
    public class InfoBar
    {
        internal IVsInfoBarUIFactory UIFactory { get; }
        internal IVsInfoBarHost Host { get; }
        internal WebBrowser WebBrowser { get; }

        internal InfoBar(IVsInfoBarUIFactory uiFactory, IVsInfoBarHost host, WebBrowser webBrowser)
        {
            UIFactory = uiFactory;
            Host = host;
            WebBrowser = webBrowser;
        }

        private class InternalInfoMessageBuilder : IBaseInfoMessageBuilder, IMoreContentInfoMessageBuilder
        {
            private readonly List<IVsInfoBarTextSpan> _textSpans;
            private readonly List<IVsInfoBarActionItem> _actionButtons;
            private ImageMoniker _image;

            private readonly InfoBar _infoBar;
            private readonly bool _hasCloseButton;

            private bool _containsHyperlink;

            internal InternalInfoMessageBuilder(InfoBar infoBar, bool hasCloseButton)
            {
                _textSpans = new List<IVsInfoBarTextSpan>();
                _actionButtons = new List<IVsInfoBarActionItem>();
                _hasCloseButton = hasCloseButton;
                _infoBar = infoBar;
            }

            public IContentInfoMessageBuilder WithInfoImage()
                => WithImage(KnownMonikers.StatusInformation);

            public IContentInfoMessageBuilder WithWarningImage()
                => WithImage(KnownMonikers.StatusWarning);

            public IContentInfoMessageBuilder WithErrorImage()
                => WithImage(KnownMonikers.StatusError);

            public IContentInfoMessageBuilder WithImage(ImageMoniker image)
            {
                _image = image;

                return this;
            }

            public IMoreContentInfoMessageBuilder WithText(string text, bool bold = false, bool italic = false, bool underline = false)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{nameof(text)} can not be null or empty.");
                }

                _textSpans.Add(new InfoBarTextSpan(text, bold, italic, underline));

                return this;
            }

            public IMoreContentInfoMessageBuilder WithHyperlink(string text, Uri uri, bool openInternally = false)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{nameof(text)} can not be null or empty.");
                }

                _textSpans.Add(new InfoBarHyperlink(text, new HyperLink(uri, openInternally)));

                _containsHyperlink = true;

                return this;
            }

            public IMoreContentInfoMessageBuilder WithHyperlink(string text, Action callback)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{nameof(text)} can not be null or empty.");
                }

                _textSpans.Add(new InfoBarHyperlink(text, new ActionCallback(callback, false)));

                _containsHyperlink = true;

                return this;
            }

            public IButtonInfoMessageBuilder WithButton(string text, bool closeAfterClick = true)
                => WithButton(text, null, closeAfterClick);

            public IButtonInfoMessageBuilder WithButton(string text, Action callback, bool closeAfterClick = true)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{nameof(text)} can not be null or empty.");
                }

                _actionButtons.Add(new InfoBarButton(text, new ActionCallback(callback, closeAfterClick)));

                return this;
            }

            public IButtonIdentifierInfoMessageBuilder<TIdentifier> WithButton<TIdentifier>(string text, TIdentifier identifier)
            {
                var builder = new InternalButtonIdentifierBuilder<TIdentifier>(_textSpans, _actionButtons, _image, _infoBar, _hasCloseButton, _containsHyperlink);

                return builder.WithButton(text, identifier);
            }

            public InfoBarHandle Publish()
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var model = new InfoBarModel(_textSpans, _actionButtons, _image, _hasCloseButton);

                var uiElement = _infoBar.UIFactory.CreateInfoBar(model);

                _infoBar.Host.AddInfoBar(uiElement);

                InfoBarEvents events = null;

                if (_containsHyperlink || _actionButtons.Count > 0)
                {
                    events = new InfoBarEvents(_infoBar, uiElement);
                }

                return new InfoBarHandle(uiElement, events);
            }
        }

        private class InternalButtonIdentifierBuilder<TIdentifier> : IButtonIdentifierInfoMessageBuilder<TIdentifier>
        {
            private readonly List<IVsInfoBarTextSpan> _textSpans;
            private readonly List<IVsInfoBarActionItem> _actionButtons;
            private ImageMoniker _image;

            private readonly InfoBar _infoBar;
            private readonly bool _hasCloseButton;
            private readonly bool _containsHyperlink;

            internal InternalButtonIdentifierBuilder(List<IVsInfoBarTextSpan> textSpans, List<IVsInfoBarActionItem> actionButtons, ImageMoniker image, InfoBar infoBar, bool hasCloseButton, bool containsHyperlink)
            {
                _textSpans = textSpans;
                _actionButtons = actionButtons;
                _image = image;
                _infoBar = infoBar;
                _hasCloseButton = hasCloseButton;
                _containsHyperlink = containsHyperlink;
            }

            public IButtonIdentifierInfoMessageBuilder<TIdentifier> WithButton(string text, TIdentifier identifier)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{nameof(text)} can not be null or empty.");
                }

                _actionButtons.Add(new InfoBarButton(text, identifier));

                return this;
            }

            public ResultInfoBarHandle<TIdentifier> Publish()
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var model = new InfoBarModel(_textSpans, _actionButtons, _image, _hasCloseButton);

                var uiElement = _infoBar.UIFactory.CreateInfoBar(model);

                _infoBar.Host.AddInfoBar(uiElement);

                ResultInfoBarEvents<TIdentifier> events = null;

                if (_containsHyperlink || _actionButtons.Count > 0)
                {
                    events = new ResultInfoBarEvents<TIdentifier>(_infoBar, uiElement);
                }

                return new ResultInfoBarHandle<TIdentifier>(uiElement, events);
            }
        }

        public IBaseInfoMessageBuilder NewMessage(bool hasCloseButton = true)
            => new InternalInfoMessageBuilder(this, hasCloseButton);

        public void RemoveMessage(InfoBarHandle handle)
            => RemoveMessage(handle.UIElement, handle.Events);

        internal void RemoveMessage(IVsInfoBarUIElement uiElement, BaseInfoBarEvents events)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            uiElement.Close();
            events?.Dispose();

            // Calling the IVsInfoBarHost::RemoveInfoBar causes the Editor to produce weird issues, such as preventing some keyboard inputs,
            // However the IVsInfoBarUIElement::Close method will do similar, it will call its own Close method.
            // Host.RemoveInfoBar(uiElement);
        }
    }
}