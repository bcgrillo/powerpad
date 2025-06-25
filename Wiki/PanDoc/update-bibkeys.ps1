# Pega aquí el contenido CSV (sin encabezado) entre @"
$csv = @"
old_key,new_key
llama_cpp_2025,repo_llama_cpp_2025
connatser,press_llamafile_performance_2024
ollama,product_ollama
ollama_integrations,repo_ollama_integrations_2025
john_MVVM_2005,press_mvvm_wpf_2005
windows_sdk,product_windows_sdk
net_8,product_dotnet8
comprehensive_2023,article_aigc_survey_2023
murphy_introduction_2000,book_ai_robotics_2000
historia_ia_2018,press_historia_ia_2018
tealab_time_2018,article_time_series_ann_2018
Hinton2006,article_deep_belief_nets_2006
ibm_cnn_2021,press_cnn_ibm_2021
GAN_2014,article_gan_2014
gpt-2_2024,press_gpt2_release_2024
dalle_2022,press_dalle_2022
gpt-3_2024,press_gpt3_apps_2024
bloom,press_bloom_2022
stable_2025,press_stable_diffusion_2022
TinyLlama,repo_tinyllama
llama_2023,press_llama_history_2023
phi-2_2023,press_phi2_2023
Lu2024SmallLM,article_small_lm_2024
destilación_2024,press_knowledge_distillation_ibm_2024
o1_2024,press_reasoning_llm_openai_2024
press_claude3_2024,press_claude3_2024
tokenization,doc_tokenization_mistral
attention_2023,article_attention_2023
tiktoken_encoding,doc_tiktoken_encoding_2022
espejel_getting_2022,press_embeddings_hf_2022
nachane_few_2024,article_fewshot_medical_2024
fine-tuning_hf,doc_finetuning_hf
ibm_attention_2024,press_attention_ibm_2024
fig_2_1,fig_word_embedding_2025
fig_2_2,press_sparse_attention_2021
jurafsky_speech_2009,book_speech_language_2009
textsynth_completion,doc_text_completion_textsynth
martineau_what_2021,press_ai_inferencing_ibm_2021
templates_hf,doc_templates_hf
quantization_hf,doc_quantization_hf
bergmann_imb_2023,press_knowledge_distillation_ibm_2023
gguf_2024,press_gguf_vs_ggml_2024
hugging_2025,company_huggingface_2025
onnx,product_onnx
onnx_2025,repo_onnx_2025
kansal_understanding_2024,press_gguf_guide_2024
weizenbaum_computer_1976,book_computer_power_1976
openai,company_openai
chatgpt,product_chatgpt
metai_2025,company_metaai_2025
lechat_2025,product_lechat_mistral_2025
gemini,press_gemini_2023
gemini_webapp,product_gemini_webapp
claude,product_claude_anthropic
claude_webapp,product_claude_webapp
deepseek_ai,company_deepseek_ai
deepseek_webapp,product_deepseek_webapp
deepseek-r1_2025,repo_deepseek_r1_2025
copilot_2025,product_copilot_2025
metaai_whatsapp_2025,press_metaai_whatsapp_2025
github_2025,press_github_copilot_2025
openai_canvas,press_canvas_openai_2024
modelcontextprotocol_2025,doc_modelcontextprotocol_2025
huggingface_tools,doc_tools_hf
hoffman_ai_2025,press_npu_pcworld_2025
mistral,company_mistral_ai
o3_o4,press_openai_o3o4_2025
NPUs,doc_npus_microsoft
notion,product_notion_ai
le_chat,press_lechat_mistral_2024
bastarrica_ranking_2025,press_ranking_chatbots_2025
bailyn_top_2025,press_top_chatbots_2025
localllama_reddit,product_localllama_reddit
WebUI,product_openwebui
enchanted_2025,repo_enchanted_2025
hollama_2025,repo_hollama_2025
librechat,product_librechat
lm_studio,product_lmstudio
msty,product_msty
gpt4all_2025,repo_gpt4all_2025
jan_ai,product_jan_ai
anythingllm,product_anythingllm
winui,product_winui3_2024
github_models,press_github_models_2025
azure_github,product_azure_github
azure,product_azure_ai_foundry
Mvvm_messaging,press_mvvm_toolkit_2024
xaml,doc_xaml_2025
webview2,doc_webview2_2025
pinvoke,doc_pinvoke_2024
system.text.json,doc_system_text_json_2025
cswin32_2025,repo_cswin32_2025
docfx,doc_docfx
sandcastle,doc_sandcastle
doxygen,repo_doxygen_2025
SonarQube,product_sonarqube
net_conventions,doc_net_conventions_2025
net_naming,doc_net_naming_2023
iDisposable,doc_idisposable_2025
using,doc_using_2023
json_serializable,doc_json_serializable_2025
nuget,product_nuget
microsoft.extensions,doc_microsoft_extensions_ai_2025
extensions.ai.abstractions,repo_extensions_ai_abstractions
azure.extensions,doc_azure_extensions
microsoft.extensions.openai,repo_microsoft_extensions_openai
ollamasharp_2025,repo_ollamasharp_2025
agility,product_html_agility_pack
community_toolkit,repo_community_toolkit
toolkit_mvvm,press_toolkit_mvvm_2024
toolkit.winui,doc_toolkit_winui
toolkit.converters,doc_toolkit_converters_2024
controls.primitives,product_controls_primitives
notifyicon,repo_notifyicon
extensions.dependencyinjection,doc_extensions_dependencyinjection
winUIEx,repo_winUIEx
buildtools,product_buildtools
manifest,doc_manifest_2023
ciberseguridad_privacidad,inproc_cybersecurity_privacy_2020
ethics_AI,doc_ethics_ai_2019
gdpr,doc_gdpr_regulation_2016
lopdgdd,doc_lopdgdd_2018
solid_2024,doc_solid_2024
winui_3,product_winui3_uno
press_deepseek_r1_2025,press_deepseek_r1_2025
copilot_press_2025,press_copilot_msft_2025
gemini_press_2025,press_gemini_android_2025
fernandez_grok_2024,press_grok_ai_xataka_2024
metaai_europe_2025,press_metaai_europe_2025
grok_app,product_grok_xai
"@ | ConvertFrom-Csv

# Construir un diccionario para reemplazos rápidos
$replaceMap = @{}
foreach ($row in $csv) {
    $replaceMap[$row.old_key] = $row.new_key
}

# Buscar todos los ficheros .md en la carpeta actual y subcarpetas
Get-ChildItem -Path . -Filter *.bib -Recurse | ForEach-Object {
    $file = $_.FullName
    # Leer el contenido como UTF8 (sin BOM)
    $content = Get-Content $file -Raw -Encoding utf8

    # Reemplazar todas las claves
    foreach ($old in $replaceMap.Keys) {
        $new = $replaceMap[$old]
        # Reemplazo seguro: solo @old_key (no partes de palabras)
        #$content = $content -replace "(@$old)\b", "`@$new"
        $content = $content -replace "({$old),", "{$new,"
    }

    # Escribir el contenido actualizado, manteniendo LF
    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($false))
}
