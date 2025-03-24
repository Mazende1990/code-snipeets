package java.s2;

import java.time.Duration;
import java.time.temporal.ChronoUnit;
import java.util.Locale;
import java.util.concurrent.TimeUnit;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.boot.context.properties.PropertyMapper;
import org.springframework.boot.convert.DurationUnit;
import org.springframework.http.CacheControl;

/**
 * {@link ConfigurationProperties Configuration properties} for general web concerns.
 *
 * @author Andy Wilkinson
 * @since 2.4.0
 */
@ConfigurationProperties("spring.web")
public class WebPropertiesClaude {

    /**
     * Locale to use. By default, this locale is overridden by the "Accept-Language" header.
     */
    private Locale locale;

    /**
     * Define how the locale should be resolved.
     */
    private LocaleResolver localeResolver = LocaleResolver.ACCEPT_HEADER;

    private final Resources resources = new Resources();

    public Locale getLocale() {
        return locale;
    }

    public void setLocale(Locale locale) {
        this.locale = locale;
    }

    public LocaleResolver getLocaleResolver() {
        return localeResolver;
    }

    public void setLocaleResolver(LocaleResolver localeResolver) {
        this.localeResolver = localeResolver;
    }

    public Resources getResources() {
        return resources;
    }

    /**
     * Enumeration of supported locale resolution strategies.
     */
    public enum LocaleResolver {
        /**
         * Always use the configured locale.
         */
        FIXED,

        /**
         * Use the "Accept-Language" header or the configured locale if the header is not set.
         */
        ACCEPT_HEADER
    }

    /**
     * Properties for static resource handling.
     */
    public static class Resources {

        private static final String[] DEFAULT_RESOURCE_LOCATIONS = {
                "classpath:/META-INF/resources/",
                "classpath:/resources/",
                "classpath:/static/",
                "classpath:/public/"
        };

        /**
         * Locations of static resources. Defaults to classpath:[/META-INF/resources/,
         * /resources/, /static/, /public/].
         */
        private String[] staticLocations = DEFAULT_RESOURCE_LOCATIONS;

        /**
         * Whether to enable default resource handling.
         */
        private boolean addMappings = true;

        private boolean customized = false;

        private final Chain chain = new Chain();

        private final Cache cache = new Cache();

        public String[] getStaticLocations() {
            return staticLocations;
        }

        public void setStaticLocations(String[] staticLocations) {
            this.staticLocations = appendSlashIfNecessary(staticLocations);
            this.customized = true;
        }

        private String[] appendSlashIfNecessary(String[] locations) {
            String[] normalized = new String[locations.length];
            for (int i = 0; i < locations.length; i++) {
                String location = locations[i];
                normalized[i] = location.endsWith("/") ? location : location + "/";
            }
            return normalized;
        }

        public boolean isAddMappings() {
            return addMappings;
        }

        public void setAddMappings(boolean addMappings) {
            this.customized = true;
            this.addMappings = addMappings;
        }

        public Chain getChain() {
            return chain;
        }

        public Cache getCache() {
            return cache;
        }

        public boolean hasBeenCustomized() {
            return customized || chain.hasBeenCustomized() || cache.hasBeenCustomized();
        }

        /**
         * Configuration for the Spring Resource Handling chain.
         */
        public static class Chain {

            private boolean customized = false;

            /**
             * Whether to enable the Spring Resource Handling chain. By default, disabled
             * unless at least one strategy has been enabled.
             */
            private Boolean enabled;

            /**
             * Whether to enable caching in the Resource chain.
             */
            private boolean cache = true;

            /**
             * Whether to enable resolution of already compressed resources (gzip, brotli).
             * Checks for a resource name with the '.gz' or '.br' file extensions.
             */
            private boolean compressed = false;

            private final Strategy strategy = new Strategy();

            /**
             * Return whether the resource chain is enabled.
             * @return whether the resource chain is enabled or {@code null} if no
             * specified settings are present.
             */
            public Boolean getEnabled() {
                boolean fixedEnabled = strategy.getFixed().isEnabled();
                boolean contentEnabled = strategy.getContent().isEnabled();
                return (fixedEnabled || contentEnabled) ? Boolean.TRUE : enabled;
            }

            public boolean hasBeenCustomized() {
                return customized || strategy.hasBeenCustomized();
            }

            public void setEnabled(boolean enabled) {
                this.enabled = enabled;
                this.customized = true;
            }

            public boolean isCache() {
                return cache;
            }

            public void setCache(boolean cache) {
                this.cache = cache;
                this.customized = true;
            }

            public Strategy getStrategy() {
                return strategy;
            }

            public boolean isCompressed() {
                return compressed;
            }

            public void setCompressed(boolean compressed) {
                this.compressed = compressed;
                this.customized = true;
            }

            /**
             * Strategies for extracting and embedding a resource version in its URL path.
             */
            public static class Strategy {

                private final Fixed fixed = new Fixed();
                private final Content content = new Content();

                public Fixed getFixed() {
                    return fixed;
                }

                public Content getContent() {
                    return content;
                }

                public boolean hasBeenCustomized() {
                    return fixed.hasBeenCustomized() || content.hasBeenCustomized();
                }

                /**
                 * Version Strategy based on content hashing.
                 */
                public static class Content {

                    private boolean customized = false;

                    /**
                     * Whether to enable the content Version Strategy.
                     */
                    private boolean enabled;

                    /**
                     * Comma-separated list of patterns to apply to the content Version Strategy.
                     */
                    private String[] paths = new String[] { "/**" };

                    public boolean isEnabled() {
                        return enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.customized = true;
                        this.enabled = enabled;
                    }

                    public String[] getPaths() {
                        return paths;
                    }

                    public void setPaths(String[] paths) {
                        this.customized = true;
                        this.paths = paths;
                    }

                    public boolean hasBeenCustomized() {
                        return customized;
                    }
                }

                /**
                 * Version Strategy based on a fixed version string.
                 */
                public static class Fixed {

                    private boolean customized = false;

                    /**
                     * Whether to enable the fixed Version Strategy.
                     */
                    private boolean enabled;

                    /**
                     * Comma-separated list of patterns to apply to the fixed Version Strategy.
                     */
                    private String[] paths = new String[] { "/**" };

                    /**
                     * Version string to use for the fixed Version Strategy.
                     */
                    private String version;

                    public boolean isEnabled() {
                        return enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.customized = true;
                        this.enabled = enabled;
                    }

                    public String[] getPaths() {
                        return paths;
                    }

                    public void setPaths(String[] paths) {
                        this.customized = true;
                        this.paths = paths;
                    }

                    public String getVersion() {
                        return version;
                    }

                    public void setVersion(String version) {
                        this.customized = true;
                        this.version = version;
                    }

                    public boolean hasBeenCustomized() {
                        return customized;
                    }
                }
            }
        }

        /**
         * Cache configuration for web resources.
         */
        public static class Cache {

            private boolean customized = false;

            /**
             * Cache period for the resources served by the resource handler. If a
             * duration suffix is not specified, seconds will be used. Can be overridden
             * by the 'spring.web.resources.cache.cachecontrol' properties.
             */
            @DurationUnit(ChronoUnit.SECONDS)
            private Duration period;

            /**
             * Cache control HTTP headers, only allows valid directive combinations.
             * Overrides the 'spring.web.resources.cache.period' property.
             */
            private final CacheControl cachecontrol = new CacheControl();

            public Duration getPeriod() {
                return period;
            }

            public void setPeriod(Duration period) {
                this.customized = true;
                this.period = period;
            }

            public CacheControl getCachecontrol() {
                return cachecontrol;
            }

            public boolean hasBeenCustomized() {
                return customized || cachecontrol.hasBeenCustomized();
            }

            /**
             * Cache Control HTTP header configuration.
             */
            public static class CacheControl {

                private boolean customized = false;

                /**
                 * Maximum time the response should be cached, in seconds if no duration
                 * suffix is not specified.
                 */
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration maxAge;

                /**
                 * Indicate that the cached response can be reused only if re-validated
                 * with the server.
                 */
                private Boolean noCache;

                /**
                 * Indicate to not cache the response in any case.
                 */
                private Boolean noStore;

                /**
                 * Indicate that once it has become stale, a cache must not use the
                 * response without re-validating it with the server.
                 */
                private Boolean mustRevalidate;

                /**
                 * Indicate intermediaries (caches and others) that they should not
                 * transform the response content.
                 */
                private Boolean noTransform;

                /**
                 * Indicate that any cache may store the response.
                 */
                private Boolean cachePublic;

                /**
                 * Indicate that the response message is intended for a single user and
                 * must not be stored by a shared cache.
                 */
                private Boolean cachePrivate;

                /**
                 * Same meaning as the "must-revalidate" directive, except that it does
                 * not apply to private caches.
                 */
                private Boolean proxyRevalidate;

                /**
                 * Maximum time the response can be served after it becomes stale, in
                 * seconds if no duration suffix is not specified.
                 */
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration staleWhileRevalidate;

                /**
                 * Maximum time the response may be used when errors are encountered, in
                 * seconds if no duration suffix is not specified.
                 */
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration staleIfError;

                /**
                 * Maximum time the response should be cached by shared caches, in seconds
                 * if no duration suffix is not specified.
                 */
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration sMaxAge;

                public Duration getMaxAge() {
                    return maxAge;
                }

                public void setMaxAge(Duration maxAge) {
                    this.customized = true;
                    this.maxAge = maxAge;
                }

                public Boolean getNoCache() {
                    return noCache;
                }

                public void setNoCache(Boolean noCache) {
                    this.customized = true;
                    this.noCache = noCache;
                }

                public Boolean getNoStore() {
                    return noStore;
                }

                public void setNoStore(Boolean noStore) {
                    this.customized = true;
                    this.noStore = noStore;
                }

                public Boolean getMustRevalidate() {
                    return mustRevalidate;
                }

                public void setMustRevalidate(Boolean mustRevalidate) {
                    this.customized = true;
                    this.mustRevalidate = mustRevalidate;
                }

                public Boolean getNoTransform() {
                    return noTransform;
                }

                public void setNoTransform(Boolean noTransform) {
                    this.customized = true;
                    this.noTransform = noTransform;
                }

                public Boolean getCachePublic() {
                    return cachePublic;
                }

                public void setCachePublic(Boolean cachePublic) {
                    this.customized = true;
                    this.cachePublic = cachePublic;
                }

                public Boolean getCachePrivate() {
                    return cachePrivate;
                }

                public void setCachePrivate(Boolean cachePrivate) {
                    this.customized = true;
                    this.cachePrivate = cachePrivate;
                }

                public Boolean getProxyRevalidate() {
                    return proxyRevalidate;
                }

                public void setProxyRevalidate(Boolean proxyRevalidate) {
                    this.customized = true;
                    this.proxyRevalidate = proxyRevalidate;
                }

                public Duration getStaleWhileRevalidate() {
                    return staleWhileRevalidate;
                }

                public void setStaleWhileRevalidate(Duration staleWhileRevalidate) {
                    this.customized = true;
                    this.staleWhileRevalidate = staleWhileRevalidate;
                }

                public Duration getStaleIfError() {
                    return staleIfError;
                }

                public void setStaleIfError(Duration staleIfError) {
                    this.customized = true;
                    this.staleIfError = staleIfError;
                }

                public Duration getSMaxAge() {
                    return sMaxAge;
                }

                public void setSMaxAge(Duration sMaxAge) {
                    this.customized = true;
                    this.sMaxAge = sMaxAge;
                }

                /**
                 * Convert these properties to an HTTP Cache-Control header.
                 * @return a CacheControl instance or null if no properties are set
                 */
                public org.springframework.http.CacheControl toHttpCacheControl() {
                    PropertyMapper map = PropertyMapper.get();
                    org.springframework.http.CacheControl control = createCacheControl();
                    
                    map.from(this::getMustRevalidate).whenTrue().toCall(control::mustRevalidate);
                    map.from(this::getNoTransform).whenTrue().toCall(control::noTransform);
                    map.from(this::getCachePublic).whenTrue().toCall(control::cachePublic);
                    map.from(this::getCachePrivate).whenTrue().toCall(control::cachePrivate);
                    map.from(this::getProxyRevalidate).whenTrue().toCall(control::proxyRevalidate);
                    
                    map.from(this::getStaleWhileRevalidate).whenNonNull()
                        .to((duration) -> control.staleWhileRevalidate(duration.getSeconds(), TimeUnit.SECONDS));
                    map.from(this::getStaleIfError).whenNonNull()
                        .to((duration) -> control.staleIfError(duration.getSeconds(), TimeUnit.SECONDS));
                    map.from(this::getSMaxAge).whenNonNull()
                        .to((duration) -> control.sMaxAge(duration.getSeconds(), TimeUnit.SECONDS));
                    
                    // Return null if no properties were set
                    return control.getHeaderValue() == null ? null : control;
                }

                private org.springframework.http.CacheControl createCacheControl() {
                    if (Boolean.TRUE.equals(noStore)) {
                        return org.springframework.http.CacheControl.noStore();
                    }
                    if (Boolean.TRUE.equals(noCache)) {
                        return org.springframework.http.CacheControl.noCache();
                    }
                    if (maxAge != null) {
                        return org.springframework.http.CacheControl.maxAge(maxAge.getSeconds(), TimeUnit.SECONDS);
                    }
                    return org.springframework.http.CacheControl.empty();
                }

                public boolean hasBeenCustomized() {
                    return customized;
                }
            }
        }
    }
}