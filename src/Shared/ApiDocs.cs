namespace Telehealth.Platform.Shared;

/// <summary>
/// Shared OpenAPI documentation UI generator.
/// Uses ReDoc (programmatic init only — the web component variant triggers a
/// removeChild bug in ReDoc 2.x when combined with Redoc.init()).
/// </summary>
public static class ApiDocs
{
    public static string GetHtml(string title) => $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>{{title}}</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet" />
    <style>
        * { box-sizing: border-box; }
        body { margin: 0; padding: 0; font-family: 'Inter', system-ui, -apple-system, sans-serif; background: #fafbfc; }
        #redoc-container { width: 100%; height: 100vh; }
    </style>
</head>
<body>
    <div id="redoc-container"></div>
    <script src="https://cdn.redocly.com/redoc/v2.1.5/bundles/redoc.standalone.js"></script>
    <script>
        (function () {
            var container = document.getElementById('redoc-container');
            if (typeof Redoc === 'undefined') {
                container.innerHTML = '<div style="padding:24px;font-family:Inter,sans-serif;color:#ef4444">Failed to load ReDoc from CDN. Check your network and reload the page.</div>';
                return;
            }
            try {
                Redoc.init(
                    '/openapi/v1/document.json',
                    {
                        scrollYOffset: 0,
                        hideDownloadButton: false,
                        expandResponses: '200,201',
                        pathInMiddlePanel: true,
                        sortPropsAlphabetically: false,
                        requiredPropsFirst: true,
                        showExtensions: true,
                        nativeScrollbars: false,
                        theme: {
                            colors: {
                                primary: { main: '#2563eb' },
                                success: { main: '#10b981' },
                                warning: { main: '#f59e0b' },
                                error:   { main: '#ef4444' }
                            },
                            typography: {
                                fontSize: '15px',
                                fontFamily: 'Inter, system-ui, sans-serif',
                                headings: { fontFamily: 'Inter, system-ui, sans-serif', fontWeight: '600' },
                                code:     { fontFamily: 'JetBrains Mono, monospace', fontSize: '13px' }
                            },
                            sidebar: {
                                backgroundColor: '#ffffff',
                                textColor: '#1f2937',
                                activeTextColor: '#2563eb',
                                width: '280px'
                            },
                            rightPanel: {
                                backgroundColor: '#1f2937',
                                textColor: '#f9fafb',
                                width: '40%'
                            }
                        }
                    },
                    container
                );
            } catch (err) {
                container.innerHTML = '<div style="padding:24px;font-family:Inter,sans-serif;color:#ef4444">ReDoc init failed: ' + (err && err.message ? err.message : err) + '</div>';
                console.error('ReDoc init failed', err);
            }
        })();
    </script>
</body>
</html>
""";
}
