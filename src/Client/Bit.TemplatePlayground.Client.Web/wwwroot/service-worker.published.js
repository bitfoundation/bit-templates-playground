// bit version: 9.0.0
// https://github.com/bitfoundation/bitplatform/tree/develop/src/Bswup


self.assetsInclude = [];
self.assetsExclude = [
    /bit\.blazorui\.fluent\.css$/,
    /bit\.blazorui\.fluent-dark\.css$/,
    /bit\.blazorui\.fluent-light\.css$/
];
self.externalAssets = [
    {
        "url": "/"
    },
    {
        url: "_framework/blazor.web.js?ver=9.0.0"
    },
    {
        "url": "Bit.TemplatePlayground.Server.Web.styles.css"
    }
];

self.serverHandledUrls = [
    /\/api\//,
    /\/odata\//,
    /\/jobs\//,
    /\/core\//,
    /\/healthchecks-ui/,
    /\/healthz/,
    /\/swagger/,
    /\/signin-/,
    /\/.well-known/,
    /\/sitemap.xml/,
];

self.defaultUrl = "/";
self.isPassive = true;
self.errorTolerance = 'lax';
self.caseInsensitiveUrl = true;


// on apps with Prerendering enabled, to have the best experience for the end user un-comment the following two lines.
// more info: https://bitplatform.dev/bswup/service-worker
// self.noPrerenderQuery = 'no-prerender=true';
// self.disablePassiveFirstBoot = true;


self.importScripts('_content/Bit.Bswup/bit-bswup.sw.js');