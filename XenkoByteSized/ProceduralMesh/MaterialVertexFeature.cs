﻿using System.ComponentModel;
using Xenko.Core;
using Xenko.Core.Annotations;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;

namespace XenkoByteSized.ProceduralMesh {

    [DataContract("MaterialVertexFeature")]
    [Display("Vertex Shader")]
    public class MaterialVertexFeature : MaterialFeature, IMaterialDisplacementFeature {

        public const string OurVertexStream = "OurPosition";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialVertexFeature"/> class.
        /// </summary>
        public MaterialVertexFeature() : this(new ComputeShaderClassColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialVertexFeature"/> class.
        /// </summary>
        /// <param name="vertexShader">The displacement map.</param>
        public MaterialVertexFeature(ComputeShaderClassColor vertexShader) {
            VertexShader = vertexShader;
            Stage = DisplacementMapStage.Vertex;
        }

        /// <summary>
        /// Gets or sets the vertex shader.
        /// </summary>
        /// <value>The vertex shader.</value>
        /// <userdoc>
        /// The shader that outputs the new position for each vertex.
        /// </userdoc>
        [DataMember(10)]
        [Display("Vertex Shader")]
        [NotNull]
        public ComputeShaderClassColor VertexShader { get; set; }

        /// <userdoc>
        /// The value indicating in which stage the vertex shader will run.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(true)]
        [Display("Shader Stage")]
        public DisplacementMapStage Stage { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context) {

            if (VertexShader == null) { return; }

            var materialStage = (MaterialShaderStage)Stage;

            // reset the displacement streams at the beginning of the stage
            context.AddStreamInitializer(materialStage, "OurVertexStream");

            // use... (FIXME: is this necessary? why?)
            context.UseStream(materialStage, OurVertexStream);

            // build the vertex shader
            var vertexShader = VertexShader;

            // Workaround to inform compute colors that sampling is occurring from a vertex shader
            context.IsNotPixelStage = materialStage != MaterialShaderStage.Pixel;

            // FIXME: this probably needs to be looked at upon returning...
            context.SetStream(materialStage, OurVertexStream, vertexShader, MaterialKeys.GenericTexture, MaterialKeys.GenericValueVector4);

            context.IsNotPixelStage = false;

            // var scaleNormal = materialStage != MaterialShaderStage.Vertex;
            var positionMember = materialStage == MaterialShaderStage.Vertex ? "Position" : "PositionWS";
            var normalMember = materialStage == MaterialShaderStage.Vertex ? "meshNormal" : "normalWS";
            context.SetStreamFinalModifier<MaterialVertexFeature>(materialStage, new ShaderClassSource("MaterialVertexDisplacement", positionMember, normalMember));

        }

    }
}
