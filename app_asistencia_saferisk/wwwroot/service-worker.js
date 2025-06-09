const CACHE_NAME = "asistencia-saferisk-v1";
const urlsToCache = [
    "/",
    "/css/velzon.min.css",
    "/css/site.css",
    "/js/layout.js",
    "/js/site.js",
    "/assets/images/cabra192.png",
    "/assets/images/cabra512.png"
];

// Instala el service worker y cachea los archivos clave
self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(urlsToCache))
    );
});

// Intercepta requests: primero busca en cache, si no, busca en red
self.addEventListener("fetch", event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});

// Limpia cache viejo en actualización
self.addEventListener("activate", event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            )
        )
    );
});
