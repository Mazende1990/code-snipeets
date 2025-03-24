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
 * @author Andy Wilkinson
 * @since 2.4.0
 */
@ConfigurationProperties("spring.web")
public class WebProperties {

    private Locale locale;
    private LocaleResolver localeResolver = LocaleResolver.ACCEPT_HEADER;
    private final Resources resources = new Resources();

    public Locale getLocale() {
        return this.locale;
    }

    public void setLocale(Locale locale) {
        this.locale = locale;
    }

    public LocaleResolver getLocaleResolver() {
        return this.localeResolver;
    }

    public void setLocaleResolver(LocaleResolver localeResolver) {
        this.localeResolver = localeResolver;
    }

    public Resources getResources() {
        return this.resources;
    }

    public enum LocaleResolver {
        FIXED, ACCEPT_HEADER
    }

    public static class Resources {

        private static final String[] DEFAULT_STATIC_LOCATIONS = {
            "classpath:/META-INF/resources/",
            "classpath:/resources/",
            "classpath:/static/",
            "classpath:/public/"
        };

        private String[] staticLocations = DEFAULT_STATIC_LOCATIONS;
        private boolean addMappings = true;
        private boolean customized = false;

        private final Chain chain = new Chain();
        private final Cache cache = new Cache();

        public String[] getStaticLocations() {
            return this.staticLocations;
        }

        public void setStaticLocations(String[] staticLocations) {
            this.staticLocations = appendTrailingSlashes(staticLocations);
            this.customized = true;
        }

        private String[] appendTrailingSlashes(String[] locations) {
            String[] updatedLocations = new String[locations.length];
            for (int i = 0; i < locations.length; i++) {
                updatedLocations[i] = locations[i].endsWith("/") ? locations[i] : locations[i] + "/";
            }
            return updatedLocations;
        }

        public boolean isAddMappings() {
            return this.addMappings;
        }

        public void setAddMappings(boolean addMappings) {
            this.addMappings = addMappings;
            this.customized = true;
        }

        public Chain getChain() {
            return this.chain;
        }

        public Cache getCache() {
            return this.cache;
        }

        public boolean hasBeenCustomized() {
            return this.customized || chain.hasBeenCustomized() || cache.hasBeenCustomized();
        }

        public static class Chain {
            private boolean customized = false;
            private Boolean enabled;
            private boolean cache = true;
            private boolean compressed = false;

            private final Strategy strategy = new Strategy();

            public Boolean getEnabled() {
                return determineEnabled(strategy.getFixed().isEnabled(), strategy.getContent().isEnabled(), this.enabled);
            }

            public void setEnabled(boolean enabled) {
                this.enabled = enabled;
                this.customized = true;
            }

            public boolean isCache() {
                return this.cache;
            }

            public void setCache(boolean cache) {
                this.cache = cache;
                this.customized = true;
            }

            public boolean isCompressed() {
                return this.compressed;
            }

            public void setCompressed(boolean compressed) {
                this.compressed = compressed;
                this.customized = true;
            }

            public Strategy getStrategy() {
                return this.strategy;
            }

            public boolean hasBeenCustomized() {
                return this.customized || strategy.hasBeenCustomized();
            }

            private static Boolean determineEnabled(boolean fixed, boolean content, Boolean chain) {
                return (fixed || content) ? Boolean.TRUE : chain;
            }

            public static class Strategy {
                private final Fixed fixed = new Fixed();
                private final Content content = new Content();

                public Fixed getFixed() {
                    return this.fixed;
                }

                public Content getContent() {
                    return this.content;
                }

                public boolean hasBeenCustomized() {
                    return fixed.hasBeenCustomized() || content.hasBeenCustomized();
                }

                public static class Content {
                    private boolean customized = false;
                    private boolean enabled;
                    private String[] paths = { "/**" };

                    public boolean isEnabled() {
                        return this.enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.customized = true;
                        this.enabled = enabled;
                    }

                    public String[] getPaths() {
                        return this.paths;
                    }

                    public void setPaths(String[] paths) {
                        this.customized = true;
                        this.paths = paths;
                    }

                    public boolean hasBeenCustomized() {
                        return this.customized;
                    }
                }

                public static class Fixed {
                    private boolean customized = false;
                    private boolean enabled;
                    private String[] paths = { "/**" };
                    private String version;

                    public boolean isEnabled() {
                        return this.enabled;
                    }

                    public void setEnabled(boolean enabled) {
                        this.customized = true;
                        this.enabled = enabled;
                    }

                    public String[] getPaths() {
                        return this.paths;
                    }

                    public void setPaths(String[] paths) {
                        this.customized = true;
                        this.paths = paths;
                    }

                    public String getVersion() {
                        return this.version;
                    }

                    public void setVersion(String version) {
                        this.customized = true;
                        this.version = version;
                    }

                    public boolean hasBeenCustomized() {
                        return this.customized;
                    }
                }
            }
        }

        public static class Cache {
            private boolean customized = false;
            private Duration period;
            private final Cachecontrol cachecontrol = new Cachecontrol();

            public Duration getPeriod() {
                return this.period;
            }

            public void setPeriod(Duration period) {
                this.customized = true;
                this.period = period;
            }

            public Cachecontrol getCachecontrol() {
                return this.cachecontrol;
            }

            public boolean hasBeenCustomized() {
                return this.customized || cachecontrol.hasBeenCustomized();
            }

            public static class Cachecontrol {
                private boolean customized = false;
                private Duration maxAge;
                private Boolean noCache;
                private Boolean noStore;
                private Boolean mustRevalidate;
                private Boolean noTransform;
                private Boolean cachePublic;
                private Boolean cachePrivate;
                private Boolean proxyRevalidate;
                private Duration staleWhileRevalidate;
                private Duration staleIfError;
                private Duration sMaxAge;

                public CacheControl toHttpCacheControl() {
                    PropertyMapper mapper = PropertyMapper.get();
                    CacheControl control = createBaseCacheControl();

                    mapper.from(this::getMustRevalidate).whenTrue().toCall(control::mustRevalidate);
                    mapper.from(this::getNoTransform).whenTrue().toCall(control::noTransform);
                    mapper.from(this::getCachePublic).whenTrue().toCall(control::cachePublic);
                    mapper.from(this::getCachePrivate).whenTrue().toCall(control::cachePrivate);
                    mapper.from(this::getProxyRevalidate).whenTrue().toCall(control::proxyRevalidate);
                    mapper.from(this::getStaleWhileRevalidate).whenNonNull()
                            .to(duration -> control.staleWhileRevalidate(duration.getSeconds(), TimeUnit.SECONDS));
                    mapper.from(this::getStaleIfError).whenNonNull()
                            .to(duration -> control.staleIfError(duration.getSeconds(), TimeUnit.SECONDS));
                    mapper.from(this::getSMaxAge).whenNonNull()
                            .to(duration -> control.sMaxAge(duration.getSeconds(), TimeUnit.SECONDS));

                    return control.getHeaderValue() == null ? null : control;
                }

                private CacheControl createBaseCacheControl() {
                    if (Boolean.TRUE.equals(this.noStore)) return CacheControl.noStore();
                    if (Boolean.TRUE.equals(this.noCache)) return CacheControl.noCache();
                    return (this.maxAge != null) ? CacheControl.maxAge(this.maxAge.getSeconds(), TimeUnit.SECONDS) : CacheControl.empty();
                }

                public boolean hasBeenCustomized() {
                    return this.customized;
                }
            }
        }
    }
}