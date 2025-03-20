package java;

import java.time.Duration;
import java.time.temporal.ChronoUnit;
import java.util.Locale;
import java.util.concurrent.TimeUnit;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.boot.context.properties.PropertyMapper;
import org.springframework.boot.convert.DurationUnit;
import org.springframework.http.CacheControl;

@ConfigurationProperties("spring.web")
public class WebPropertiesDeep {

    private Locale locale;
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

    public enum LocaleResolver {
        FIXED, ACCEPT_HEADER
    }

    public static class Resources {

        private static final String[] CLASSPATH_RESOURCE_LOCATIONS = {
            "classpath:/META-INF/resources/", "classpath:/resources/", "classpath:/static/", "classpath:/public/"
        };

        private String[] staticLocations = CLASSPATH_RESOURCE_LOCATIONS;
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
                normalized[i] = locations[i].endsWith("/") ? locations[i] : locations[i] + "/";
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

        public static class Chain {

            private Boolean enabled;
            private boolean cache = true;
            private boolean compressed = false;
            private boolean customized = false;
            private final Strategy strategy = new Strategy();

            public Boolean getEnabled() {
                return getEnabled(strategy.getFixed().isEnabled(), strategy.getContent().isEnabled(), enabled);
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

            private boolean hasBeenCustomized() {
                return customized || strategy.hasBeenCustomized();
            }

            static Boolean getEnabled(boolean fixedEnabled, boolean contentEnabled, Boolean chainEnabled) {
                return (fixedEnabled || contentEnabled) ? Boolean.TRUE : chainEnabled;
            }

            public static class Strategy {

                private final Fixed fixed = new Fixed();
                private final Content content = new Content();

                public Fixed getFixed() {
                    return fixed;
                }

                public Content getContent() {
                    return content;
                }

                private boolean hasBeenCustomized() {
                    return fixed.hasBeenCustomized() || content.hasBeenCustomized();
                }

                public static class Content {

                    private boolean enabled;
                    private String[] paths = { "/**" };
                    private boolean customized = false;

                    public boolean isEnabled() {
                        return enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.enabled = enabled;
                        this.customized = true;
                    }

                    public String[] getPaths() {
                        return paths;
                    }

                    public void setPaths(String[] paths) {
                        this.paths = paths;
                        this.customized = true;
                    }

                    private boolean hasBeenCustomized() {
                        return customized;
                    }
                }

                public static class Fixed {

                    private boolean enabled;
                    private String[] paths = { "/**" };
                    private String version;
                    private boolean customized = false;

                    public boolean isEnabled() {
                        return enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.enabled = enabled;
                        this.customized = true;
                    }

                    public String[] getPaths() {
                        return paths;
                    }

                    public void setPaths(String[] paths) {
                        this.paths = paths;
                        this.customized = true;
                    }

                    public String getVersion() {
                        return version;
                    }

                    public void setVersion(String version) {
                        this.version = version;
                        this.customized = true;
                    }

                    private boolean hasBeenCustomized() {
                        return customized;
                    }
                }
            }
        }

        public static class Cache {

            @DurationUnit(ChronoUnit.SECONDS)
            private Duration period;
            private boolean customized = false;
            private final Cachecontrol cachecontrol = new Cachecontrol();

            public Duration getPeriod() {
                return period;
            }

            public void setPeriod(Duration period) {
                this.period = period;
                this.customized = true;
            }

            public Cachecontrol getCachecontrol() {
                return cachecontrol;
            }

            private boolean hasBeenCustomized() {
                return customized || cachecontrol.hasBeenCustomized();
            }

            public static class Cachecontrol {

                @DurationUnit(ChronoUnit.SECONDS)
                private Duration maxAge;
                private Boolean noCache;
                private Boolean noStore;
                private Boolean mustRevalidate;
                private Boolean noTransform;
                private Boolean cachePublic;
                private Boolean cachePrivate;
                private Boolean proxyRevalidate;
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration staleWhileRevalidate;
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration staleIfError;
                @DurationUnit(ChronoUnit.SECONDS)
                private Duration sMaxAge;
                private boolean customized = false;

                public Duration getMaxAge() {
                    return maxAge;
                }

                public void setMaxAge(Duration maxAge) {
                    this.maxAge = maxAge;
                    this.customized = true;
                }

                public Boolean getNoCache() {
                    return noCache;
                }

                public void setNoCache(Boolean noCache) {
                    this.noCache = noCache;
                    this.customized = true;
                }

                public Boolean getNoStore() {
                    return noStore;
                }

                public void setNoStore(Boolean noStore) {
                    this.noStore = noStore;
                    this.customized = true;
                }

                public Boolean getMustRevalidate() {
                    return mustRevalidate;
                }

                public void setMustRevalidate(Boolean mustRevalidate) {
                    this.mustRevalidate = mustRevalidate;
                    this.customized = true;
                }

                public Boolean getNoTransform() {
                    return noTransform;
                }

                public void setNoTransform(Boolean noTransform) {
                    this.noTransform = noTransform;
                    this.customized = true;
                }

                public Boolean getCachePublic() {
                    return cachePublic;
                }

                public void setCachePublic(Boolean cachePublic) {
                    this.cachePublic = cachePublic;
                    this.customized = true;
                }

                public Boolean getCachePrivate() {
                    return cachePrivate;
                }

                public void setCachePrivate(Boolean cachePrivate) {
                    this.cachePrivate = cachePrivate;
                    this.customized = true;
                }

                public Boolean getProxyRevalidate() {
                    return proxyRevalidate;
                }

                public void setProxyRevalidate(Boolean proxyRevalidate) {
                    this.proxyRevalidate = proxyRevalidate;
                    this.customized = true;
                }

                public Duration getStaleWhileRevalidate() {
                    return staleWhileRevalidate;
                }

                public void setStaleWhileRevalidate(Duration staleWhileRevalidate) {
                    this.staleWhileRevalidate = staleWhileRevalidate;
                    this.customized = true;
                }

                public Duration getStaleIfError() {
                    return staleIfError;
                }

                public void setStaleIfError(Duration staleIfError) {
                    this.staleIfError = staleIfError;
                    this.customized = true;
                }

                public Duration getSMaxAge() {
                    return sMaxAge;
                }

                public void setSMaxAge(Duration sMaxAge) {
                    this.sMaxAge = sMaxAge;
                    this.customized = true;
                }

                public CacheControl toHttpCacheControl() {
                    PropertyMapper map = PropertyMapper.get();
                    CacheControl control = createCacheControl();
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
                    if (control.getHeaderValue() == null) {
                        return null;
                    }
                    return control;
                }

                private CacheControl createCacheControl() {
                    if (Boolean.TRUE.equals(noStore)) {
                        return CacheControl.noStore();
                    }
                    if (Boolean.TRUE.equals(noCache)) {
                        return CacheControl.noCache();
                    }
                    if (maxAge != null) {
                        return CacheControl.maxAge(maxAge.getSeconds(), TimeUnit.SECONDS);
                    }
                    return CacheControl.empty();
                }

                private boolean hasBeenCustomized() {
                    return customized;
                }
            }
        }
    }
}