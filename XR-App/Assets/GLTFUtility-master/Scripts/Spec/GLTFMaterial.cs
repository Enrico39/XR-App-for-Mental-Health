using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using Newtonsoft.Json.Linq;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#material
	[Preserve] public class GLTFMaterial {
#if UNITY_EDITOR
		public static Material defaultMaterial { get { return _defaultMaterial != null ? _defaultMaterial : _defaultMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"); } }
		private static Material _defaultMaterial;
#else
		public static Material defaultMaterial { get { return null; } }
#endif

		public string name;
		public PbrMetalRoughness pbrMetallicRoughness;
		public TextureInfo normalTexture;
		public TextureInfo occlusionTexture;
		public TextureInfo emissiveTexture;
		[JsonConverter(typeof(ColorRGBConverter))] public Color emissiveFactor = Color.black;
		[JsonConverter(typeof(EnumConverter))] public AlphaMode alphaMode = AlphaMode.OPAQUE;
		public float alphaCutoff = 0.5f;
		public bool doubleSided = false;
		public Extensions extensions;
		public JObject extras;

		public class ImportResult {
			public Material material;
		}
		
		public IEnumerator CreateMaterial(GLTFTexture.ImportResult[] textures, ShaderSettings shaderSettings, Action<Material> onFinish) {
    Material mat = null;
    IEnumerator en = null;

    try {
        Shader unlitShader = Shader.Find("Unlit/Texture");
        if (unlitShader == null) {
            throw new ArgumentNullException("shader", "Shader 'Unlit/Texture' non trovato.");
        }
        mat = new Material(unlitShader);
    } catch (Exception ex) {
        Debug.LogError($"Errore durante l'inizializzazione dello shader: {ex.Message}");
        yield break;
    }

    // Diffuse/BaseColor Texture
    if (pbrMetallicRoughness?.baseColorTexture != null) {
        Debug.Log("Caricamento della Base Color Texture...");
        en = TryGetTexture(textures, pbrMetallicRoughness.baseColorTexture, false, tex => {
            if (tex != null) {
                mat.mainTexture = tex;
                Debug.Log("Base Color Texture applicata con successo.");
            } else {
                Debug.LogWarning("Base Color Texture non caricata correttamente.");
            }
        });
        while (en.MoveNext()) { yield return null; }
    } else {
        Debug.LogWarning("Nessuna Base Color Texture trovata. Verrà utilizzato un colore bianco.");
    }

    // Normal Texture
    if (normalTexture != null) {
        Debug.Log("Caricamento della normal texture...");
        en = TryGetTexture(textures, normalTexture, true, tex => {
            if (tex != null) {
                mat.SetTexture("_BumpMap", tex);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetFloat("_BumpScale", normalTexture.scale);
                if (normalTexture.extensions != null) {
                    normalTexture.extensions.Apply(normalTexture, mat, "_BumpMap");
                }
            } else {
                Debug.LogWarning("Normal texture non caricata correttamente.");
            }
        });
        while (en.MoveNext()) { yield return null; }
    }

    // Emissive Texture
    if (emissiveTexture != null) {
        Debug.Log("Caricamento della emissive texture...");
        en = TryGetTexture(textures, emissiveTexture, false, tex => {
            if (tex != null) {
                mat.SetTexture("_EmissionMap", tex);
                mat.EnableKeyword("_EMISSION");
                if (emissiveTexture.extensions != null) {
                    emissiveTexture.extensions.Apply(emissiveTexture, mat, "_EmissionMap");
                }
            } else {
                Debug.LogWarning("Emissive texture non caricata correttamente.");
            }
        });
        while (en.MoveNext()) { yield return null; }
    }

    // Imposta il nome del materiale
    mat.name = name;
    Debug.Log($"Materiale creato con successo: {mat.name}");

    // Callback per restituire il materiale
    onFinish(mat);
}





		public static IEnumerator TryGetTexture(GLTFTexture.ImportResult[] textures, TextureInfo texture, bool linear, Action<Texture2D> onFinish, Action<float> onProgress = null) {
			if (texture == null || texture.index < 0) {
				if (onProgress != null) onProgress(1f);
				onFinish(null);
			}
			if (textures == null) {
				if (onProgress != null) onProgress(1f);
				onFinish(null);
			}
			if (textures.Length <= texture.index) {
				Debug.LogWarning("Attempted to get texture index " + texture.index + " when only " + textures.Length + " exist");
				if (onProgress != null) onProgress(1f);
				onFinish(null);
			}
			IEnumerator en = textures[texture.index].GetTextureCached(linear, onFinish, onProgress);
			while (en.MoveNext()) { yield return null; };
		}

		[Preserve] public class Extensions {
			public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness = null;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#pbrmetallicroughness
		[Preserve] public class PbrMetalRoughness {
			[JsonConverter(typeof(ColorRGBAConverter))] public Color baseColorFactor = Color.white;
			public TextureInfo baseColorTexture;
			public float metallicFactor = 1f;
			public float roughnessFactor = 1f;
			public TextureInfo metallicRoughnessTexture;

			public IEnumerator CreateMaterial(GLTFTexture.ImportResult[] textures, AlphaMode alphaMode, ShaderSettings shaderSettings, Action<Material> onFinish) {
				// Shader
				Shader sh = null;
				if (alphaMode == AlphaMode.BLEND) sh = shaderSettings.MetallicBlend;
				else sh = shaderSettings.Metallic;

				// Material
				Material mat = new Material(sh);
				mat.color = baseColorFactor;
				mat.SetFloat("_Metallic", metallicFactor);
				mat.SetFloat("_Roughness", roughnessFactor);

				// Assign textures
				if (textures != null) {
					// Base color texture
					if (baseColorTexture != null && baseColorTexture.index >= 0) {
						if (textures.Length <= baseColorTexture.index) {
							Debug.LogWarning("Attempted to get basecolor texture index " + baseColorTexture.index + " when only " + textures.Length + " exist");
						} else {
							IEnumerator en = textures[baseColorTexture.index].GetTextureCached(false, tex => {
								if (tex != null) {
									mat.SetTexture("_MainTex", tex);
									if (baseColorTexture.extensions != null) {
										baseColorTexture.extensions.Apply(baseColorTexture, mat, "_MainTex");
									}
								}
							});
							while (en.MoveNext()) { yield return null; };
						}
					}
					// Metallic roughness texture
					if (metallicRoughnessTexture != null && metallicRoughnessTexture.index >= 0) {
						if (textures.Length <= metallicRoughnessTexture.index) {
							Debug.LogWarning("Attempted to get metallicRoughness texture index " + metallicRoughnessTexture.index + " when only " + textures.Length + " exist");
						} else {
							IEnumerator en = TryGetTexture(textures, metallicRoughnessTexture, true, tex => {
								if (tex != null) {
									mat.SetTexture("_MetallicGlossMap", tex);
									mat.EnableKeyword("_METALLICGLOSSMAP");
									if (metallicRoughnessTexture.extensions != null) {
										metallicRoughnessTexture.extensions.Apply(metallicRoughnessTexture, mat, "_MetallicGlossMap");
									}
								}
							});
							while (en.MoveNext()) { yield return null; };
						}
					}
				}

				// After the texture and color is extracted from the glTFObject
				if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mat.mainTexture);
				if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColorFactor);
				onFinish(mat);
			}
		}

		[Preserve] public class PbrSpecularGlossiness {
			/// <summary> The reflected diffuse factor of the material </summary>
			[JsonConverter(typeof(ColorRGBAConverter))] public Color diffuseFactor = Color.white;
			/// <summary> The diffuse texture </summary>
			public TextureInfo diffuseTexture;
			/// <summary> The reflected diffuse factor of the material </summary>
			[JsonConverter(typeof(ColorRGBConverter))] public Color specularFactor = Color.white;
			/// <summary> The glossiness or smoothness of the material </summary>
			public float glossinessFactor = 1f;
			/// <summary> The specular-glossiness texture </summary>
			public TextureInfo specularGlossinessTexture;

			public IEnumerator CreateMaterial(GLTFTexture.ImportResult[] textures, AlphaMode alphaMode, ShaderSettings shaderSettings, Action<Material> onFinish) {
				// Shader
				Shader sh = null;
				if (alphaMode == AlphaMode.BLEND) sh = shaderSettings.SpecularBlend;
				else sh = shaderSettings.Specular;

				// Material
				Material mat = new Material(sh);
				mat.color = diffuseFactor;
				mat.SetColor("_SpecColor", specularFactor);
				mat.SetFloat("_GlossyReflections", glossinessFactor);

				// Assign textures
				if (textures != null) {
					// Diffuse texture
					if (diffuseTexture != null) {
						if (textures.Length <= diffuseTexture.index) {
							Debug.LogWarning("Attempted to get diffuseTexture texture index " + diffuseTexture.index + " when only " + textures.Length + " exist");
						} else {
							IEnumerator en = textures[diffuseTexture.index].GetTextureCached(false, tex => {
								if (tex != null) {
									mat.SetTexture("_MainTex", tex);
									if (diffuseTexture.extensions != null) {
										diffuseTexture.extensions.Apply(diffuseTexture, mat, "_MainTex");
									}
								}
							});
							while (en.MoveNext()) { yield return null; };
						}
					}
					// Specular texture
					if (specularGlossinessTexture != null) {
						if (textures.Length <= specularGlossinessTexture.index) {
							Debug.LogWarning("Attempted to get specularGlossinessTexture texture index " + specularGlossinessTexture.index + " when only " + textures.Length + " exist");
						} else {
							mat.EnableKeyword("_SPECGLOSSMAP");
							IEnumerator en = textures[specularGlossinessTexture.index].GetTextureCached(false, tex => {
								if (tex != null) {
									mat.SetTexture("_SpecGlossMap", tex);
									mat.EnableKeyword("_SPECGLOSSMAP");
									if (specularGlossinessTexture.extensions != null) {
										specularGlossinessTexture.extensions.Apply(specularGlossinessTexture, mat, "_SpecGlossMap");
									}
								}
							});
							while (en.MoveNext()) { yield return null; };
						}
					}
				}
				onFinish(mat);
			}
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#normaltextureinfo
		[Preserve] public class TextureInfo {
			[JsonProperty(Required = Required.Always)] public int index;
			public int texCoord = 0;
			public float scale = 1;
			public Extensions extensions;

			[Preserve] public class Extensions {
				public KHR_texture_transform KHR_texture_transform;

				public void Apply(GLTFMaterial.TextureInfo texInfo, Material material, string textureSamplerName) {
					// TODO: check if GLTFObject has extensionUsed/extensionRequired for these extensions

					if (KHR_texture_transform != null) {
						KHR_texture_transform.Apply(texInfo, material, textureSamplerName);
					}
				}
			}

			public interface IExtension {
				void Apply(GLTFMaterial.TextureInfo texInfo, Material material, string textureSamplerName);
			}
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			private List<GLTFMaterial> materials;
			private GLTFTexture.ImportTask textureTask;
			private ImportSettings importSettings;

			public ImportTask(List<GLTFMaterial> materials, GLTFTexture.ImportTask textureTask, ImportSettings importSettings) : base(textureTask) {
				this.materials = materials;
				this.textureTask = textureTask;
				this.importSettings = importSettings;

				task = new Task(() => {
					if (materials == null) return;
					Result = new ImportResult[materials.Count];
				});
			}

			public override IEnumerator OnCoroutine(Action<float> onProgress = null) {
				// No materials
				if (materials == null) {
					if (onProgress != null) onProgress.Invoke(1f);
					IsCompleted = true;
					yield break;
				}

				for (int i = 0; i < Result.Length; i++) {
					Result[i] = new ImportResult();

					IEnumerator en = materials[i].CreateMaterial(textureTask.Result, importSettings.shaderOverrides, x => Result[i].material = x);
					while (en.MoveNext()) { yield return null; };

					if (Result[i].material.name == null) Result[i].material.name = "material" + i;
					if (onProgress != null) onProgress.Invoke((float) (i + 1) / (float) Result.Length);
					yield return null;
				}
				IsCompleted = true;
			}
		}
	}
}