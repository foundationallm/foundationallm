<template>
    <div>
        <h2 class="page-header">Branding</h2>
        <div class="page-subheader">
            <p>Customize the look and feel of your UI.</p>
        </div>
        <div class="steps">
            <div class="step span-2" v-for="key in orderedKeys" :key="key">
                <div class="step-header mb-2" :id="key.split(':').pop()">{{ getFriendlyName(key) }}</div>
                <div class="mb-2">{{ getBrandingDescription(key) }}</div>
                <InputText :value="getBrandingValue(key)" @input="updateBrandingValue(key, $event.target.value)" :aria-labelledby="key.split(':').pop()" />
            </div>
            <div style="border-top: 3px solid #bbb;" />
            <div class="step span-2 color-group-container" v-for="group in orderedKeyColorsGrouped" :key="group.label">
                <div class="color-group">
                    <div class="step span-2" v-for="key in group.keys" :key="key.key">
                        <div class="step-header mb-2" :id="key.key.split(':').pop()">{{ getFriendlyName(key.key) }}</div>
                        <div class="mb-2">{{ getBrandingDescription(key.key) }}</div>
                        <div class="color-input-container">
                            <InputText :value="getBrandingValue(key.key)" @input="updateBrandingValue(key.key, $event.target.value)" :aria-labelledby="key.key.split(':').pop()" />
                            <ColorPicker :modelValue="getColorBrandingValue(key.key)" class="color-picker" :format="getColorBrandingFormat(key.key)" @change="updateBrandingValue(key.key, $event.value)" />
                        </div>
                    </div>
                </div>
                <div>
                    <div class="color-ratio" v-if="group.keys.find(k => k.type === 'background').key && group.keys.find(k => k.type === 'foreground')?.key">
                        {{ getContrastRatio(getBrandingValue(group.keys.find(k => k.type === 'background').key), getBrandingValue(group.keys.find(k => k.type === 'foreground')?.key) || 'transparent') }} : 1
                    </div>
                    <div class="color-preview-background" :style="{ backgroundColor: getBrandingValue(group.keys.find(k => k.type === 'background').key) }">
                        <div class="color-preview-foreground" :style="{ color: getBrandingValue(group.keys.find(k => k.type === 'foreground')?.key) || 'transparent' }">TEST</div>
                    </div>
                </div>
            </div>
            <div style="border-top: 3px solid #bbb;" />
            <div class="step span-2" v-for="key in unorderedKeys" :key="key">
                <div class="step-header mb-2" :id="key.split(':').pop()">{{ getFriendlyName(key) }}</div>
                <div class="mb-2">{{ getBrandingDescription(key) }}</div>
                <InputText :value="getBrandingValue(key)" @input="updateBrandingValue(key, $event.target.value)" :aria-labelledby="key.split(':').pop()" />
            </div>
            <div class="button-container column-2 justify-self-end">
                <Button
                    label="Cancel"
                    @click="cancelBrandingChanges"
                    severity="secondary"
                />
                <Button
                    label="Set Default"
                    @click="setDefaultBranding"
                />
                <Button
                    label="Save"
                    severity="primary"
                    @click="saveBranding"
                />
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
    name: 'Branding',

    data() {
        return {
            branding: null as any,
            brandingOriginal: null as any,
            brandingDefault: [
                {
                    "key": "FoundationaLLM:Branding:AccentColor",
                    "value": "#fff",
                },
                {
                    "key": "FoundationaLLM:Branding:AccentTextColor",
                    "value": "#131833",
                },
                {
                    "key": "FoundationaLLM:Branding:BackgroundColor",
                    "value": "#fff",
                },
                {
                    "key": "FoundationaLLM:Branding:FavIconUrl",
                    "value": "favicon.ico",
                },
                {
                    "key": "FoundationaLLM:Branding:LogoUrl",
                    "value": "foundationallm-logo-white.svg",
                },
                {
                    "key": "FoundationaLLM:Branding:PrimaryButtonBackgroundColor",
                    "value": "#5472d4",
                },
                {
                    "key": "FoundationaLLM:Branding:PrimaryButtonTextColor",
                    "value": "#fff",
                },
                {
                    "key": "FoundationaLLM:Branding:PrimaryColor",
                    "value": "#131833",
                },
                {
                    "key": "FoundationaLLM:Branding:PrimaryTextColor",
                    "value": "#fff",
                },
                {
                    "key": "FoundationaLLM:Branding:SecondaryButtonBackgroundColor",
                    "value": "#70829a",
                },
                {
                    "key": "FoundationaLLM:Branding:SecondaryButtonTextColor",
                    "value": "#fff",
                },
                {
                    "key": "FoundationaLLM:Branding:SecondaryColor",
                    "value": "#334581",
                },
                {
                    "key": "FoundationaLLM:Branding:SecondaryTextColor",
                    "value": "#fff",
                }
            ],
            orderedKeys: [
                "FoundationaLLM:Branding:CompanyName",
                "FoundationaLLM:Branding:FavIconUrl",
                "FoundationaLLM:Branding:FooterText",
                "FoundationaLLM:Branding:KioskMode",
                "FoundationaLLM:Branding:LogoText",
                "FoundationaLLM:Branding:LogoUrl",
                "FoundationaLLM:Branding:PageTitle",
            ],
            orderedKeysColors: [
                "FoundationaLLM:Branding:AccentColor",
                "FoundationaLLM:Branding:AccentTextColor",
                "FoundationaLLM:Branding:BackgroundColor",
                "FoundationaLLM:Branding:PrimaryButtonBackgroundColor",
                "FoundationaLLM:Branding:PrimaryButtonTextColor",
                "FoundationaLLM:Branding:PrimaryColor",
                "FoundationaLLM:Branding:PrimaryTextColor",
                "FoundationaLLM:Branding:SecondaryButtonBackgroundColor",
                "FoundationaLLM:Branding:SecondaryButtonTextColor",
                "FoundationaLLM:Branding:SecondaryColor",
                "FoundationaLLM:Branding:SecondaryTextColor",
            ],
            orderedKeyColorsGrouped: [
                {
                    label: 'Accent',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:AccentColor",
                            type: 'background',
                        },
                        {
                            key: "FoundationaLLM:Branding:AccentTextColor",
                            type: 'foreground',
                        }
                    ]
                },
                {
                    label: 'Background',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:BackgroundColor",
                            type: 'background',
                        }
                    ]
                },
                {
                    label: 'Primary Button',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:PrimaryButtonBackgroundColor",
                            type: 'background',
                        },
                        {
                            key: "FoundationaLLM:Branding:PrimaryButtonTextColor",
                            type: 'foreground',
                        }
                    ]
                },
                {
                    label: 'Primary',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:PrimaryColor",
                            type: 'background',
                        },
                        {
                            key: "FoundationaLLM:Branding:PrimaryTextColor",
                            type: 'foreground',
                        }
                    ]
                },
                {
                    label: 'Secondary Button',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:SecondaryButtonBackgroundColor",
                            type: 'background',
                        },
                        {
                            key: "FoundationaLLM:Branding:SecondaryButtonTextColor",
                            type: 'foreground',
                        }
                    ]
                },
                {
                    label: 'Secondary',
                    keys: [
                        {
                            key: "FoundationaLLM:Branding:SecondaryColor",
                            type: 'background',
                        },
                        {
                            key: "FoundationaLLM:Branding:SecondaryTextColor",
                            type: 'foreground',
                        }
                    ]
                },
            ],
            unorderedKeys: [] as string[],
        };
    },

    async created() {
        await this.getBranding();
    },

    methods: {
        async getBranding() {
            try {
                this.branding = await api.getBranding();
                console.log(this.branding);
                this.brandingOriginal = JSON.parse(JSON.stringify(this.branding));
                this.getUnorderedKeys();
            } catch (error) {
                this.$toast.add({
                    severity: 'error',
                    detail: error?.response?._data || error,
                    life: 5000,
                });
            }
        },

        getUnorderedKeys() {
            const keys = this.branding.map((brand: any) => brand.resource.key);
            this.unorderedKeys = keys.filter((key: string) => !this.orderedKeys.includes(key) && !this.orderedKeysColors.includes(key));
        },

        getBrandingValue(key: string) {
            const brand = this.branding?.find((item: any) => item.resource.key === key);
            return brand ? brand.resource.value : '';
        },

        getColorBrandingValue(key: string) {
            let color = null;
            const brand = this.branding?.find((item: any) => item.resource.key === key);
            if (brand && brand.resource.value) {
                let hex = brand.resource.value;
                // Check if the hex color is 3 digits
                if (/^#[0-9A-F]{3}$/i.test(hex)) {
                    // Convert 3-digit hex to 6-digit hex
                    hex = `#${hex[1]}${hex[1]}${hex[2]}${hex[2]}${hex[3]}${hex[3]}`;

                    color = hex;
                } else if (/^#[0-9A-F]{6}$/i.test(hex)) {
                    color = hex;
                } else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(hex)) {
                    // Convert rgb to object with r, g, b properties
                    const rgb = hex.match(/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i);
                    color = {
                        r: parseInt(rgb[1]),
                        g: parseInt(rgb[2]),
                        b: parseInt(rgb[3]),
                    };
                }
            } else {
                color = brand ? brand.resource.value : '';
            }
            return color ? color : '';
        },

        getColorBrandingFormat(key: string) {
            const brand = this.branding?.find((item: any) => item.resource.key === key);
            if (brand && brand.resource.value) {
                let hex = brand.resource.value;
                if (/^#[0-9A-F]{3}$/i.test(hex) || /^#[0-9A-F]{6}$/i.test(hex)) {
                    return 'hex';
                } else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(hex)) {
                    return 'rgb';
                }
            }
            return 'hex';
        },

        getLuminance(color: string) {
            let r, g, b;
            if (/^#[0-9A-F]{3}$/i.test(color)) {
                const rgb = color.match(/^#([0-9A-F]{3})$/i);
                r = parseInt(rgb[1][0] + rgb[1][0], 16);
                g = parseInt(rgb[1][1] + rgb[1][1], 16);
                b = parseInt(rgb[1][2] + rgb[1][2], 16);
            } else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(color)) {
                const rgb = color.match(/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i);
                r = parseInt(rgb[1]);
                g = parseInt(rgb[2]);
                b = parseInt(rgb[3]);
            } else if (/^#[0-9A-F]{6}$/i.test(color)) {
                r = parseInt(color.substr(1, 2), 16);
                g = parseInt(color.substr(3, 2), 16);
                b = parseInt(color.substr(5, 2), 16);
            }

            const srgbToLinear = (channel) => {
                const c = channel / 255;
                return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
            };

            const linearR = srgbToLinear(r);
            const linearG = srgbToLinear(g);
            const linearB = srgbToLinear(b);

            return 0.2126 * linearR + 0.7152 * linearG + 0.0722 * linearB;
        },

        getContrastRatio(color1: string, color2: string) {
            const lum1 = this.getLuminance(color1);
            const lum2 = this.getLuminance(color2);
            const ratio = lum1 > lum2 ? (lum1 + 0.05) / (lum2 + 0.05) : (lum2 + 0.05) / (lum1 + 0.05);

            return Math.round(ratio * 100) / 100;
        },

        getBrandingDescription(key: string) {
            const brand = this.branding?.find((item: any) => item.resource.key === key);
            return brand ? brand.resource.description : '';
        },

        getFriendlyName(key: string) {
            return key.split(':').pop()?.replace(/([A-Z])/g, ' $1').trim() || '';
        },

        updateBrandingValue(key: string, newValue: string) {
            // if newValue equals regular expression of hex color code \b[0-9a-fA-F]{6}\b
            if (/^[0-9a-fA-F]{6}$/.test(newValue)) {
                if (!newValue.startsWith("#")) {
                    newValue = "#" + newValue;
                }
            } else if (typeof newValue === 'object' && newValue !== null && 'r' in newValue && 'g' in newValue && 'b' in newValue) {
                newValue = `rgb(${newValue.r}, ${newValue.g}, ${newValue.b})`;
            }
            const brand = this.branding?.find((item: any) => item.resource.key === key);
            if (brand) {
                brand.resource.value = newValue;
            }
        },

        setDefaultBranding() {
            this.branding.forEach((brand: any) => {
                const defaultBrand = this.brandingDefault.find((defaultBrand: any) => defaultBrand.key === brand.resource.key);
                if (defaultBrand) {
                    brand.resource.value = defaultBrand.value;
                }
            });
        },

        cancelBrandingChanges() {
            this.branding = JSON.parse(JSON.stringify(this.brandingOriginal));
        },

        async saveBranding() {
            console.log(this.branding);
            console.log(this.brandingOriginal);
            const changedBranding = this.branding.filter((brand: any) => {
                const originalBrand = this.brandingOriginal.find((original: any) => original.resource.key === brand.resource.key);
                return originalBrand.resource.value !== brand.resource.value;
            });

            console.log(changedBranding);

            const promises = changedBranding.map((brand: any) => {
                const params = {
                    "type": brand.resource.type,
                    "name": brand.resource.name,
                    "display_name": brand.resource.display_name,
                    "description": brand.resource.description,
                    "key": brand.resource.key,
                    "value": brand.resource.value,
                    "content_type": brand.resource.content_type,
                };
                return api.saveBranding(brand.resource.key, params);
            });

            const results = await Promise.all(promises);
            console.log(results);
        }
    }
};
</script>

<style lang="scss">
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.button-container {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 14px;
}

.color-group-container {
    flex-direction: row !important;
    align-items: center;
    gap: 24px;
    border: 2px solid #000;
    padding: 10px;
}

.color-group {
    flex: 1;
}

.color-preview-background {
    height: 50px;
    display: flex;
    justify-content: center;
    align-items: center;
    border: 2px solid #000;
}

.color-preview-foreground {
    padding: 10px;
    font-weight: bold;
}

.color-input-container {
    display: flex;
    align-items: center;
}

.color-picker {
    width: 50px;
}

.p-colorpicker-preview {
    border-radius: 0px;
    height: 100%;
    border-left: 0px;
}

.color-ratio {
    text-align: center;
    font-weight: bold;
}
</style>