using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 5f11ca47-2841-4ece-af83-e7bc2b5e8603_fe5629e0-49e2-4e9c-be74-076aa419dcdd

namespace UWPApp
{
    public sealed class _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelOutput()
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

    public sealed class _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModel
    {
        private LearningModelPreview learningModel;
        public static async Task<_x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModel> Create_x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModel model = new _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<_x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelOutput> EvaluateAsync(_x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelInput input) {
            _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelOutput output = new _x0035_f11ca47_x002D_2841_x002D_4ece_x002D_af83_x002D_e7bc2b5e8603_fe5629e0_x002D_49e2_x002D_4e9c_x002D_be74_x002D_076aa419dcddModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
