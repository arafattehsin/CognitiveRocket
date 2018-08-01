using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 5f11ca47-2841-4ece-af83-e7bc2b5e8603_fe5629e0-49e2-4e9c-be74-076aa419dcdd

namespace UWPApp
{
    public sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "nic-classic-back", float.NaN },
                { "nic-classic-front", float.NaN },
                { "nicop-back", float.NaN },
                { "nicop-front", float.NaN },
                { "nic-smartcard-back", float.NaN },
                { "nic-smartcard-front", float.NaN },
            };
        }
    }

    public sealed class Model
    {
        private LearningModelPreview learningModel;
        public static async Task<Model> CreateModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            Model model = new Model();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<ModelOutput> EvaluateAsync(ModelInput input) {
            ModelOutput output = new ModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
