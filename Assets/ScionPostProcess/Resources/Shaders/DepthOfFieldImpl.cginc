//#define DEBUG_OPTIMIZATIONS
#define USE_SINGLE_TAP_EARLY_OUT

//Branching is a *lot* slower on OpenGL, so don't do it
#ifndef SHADER_API_OPENGL 
	#define USE_REDUCED_SAMPLES_OPTIMIZATION
#endif //SHADER_API_OPENGL 

#define USING_PRESORT
		
//If undef will use the same random offset every sample instead of recomputing
#define DOF_RANDOM_OFFSET_PER_SAMPLE
#define SAMPLE_RADIUS_RANDOM_OFFSET

#define SINGLE_TAP_EARLY_OUT_PIXEL_RADIUS 1.0f

#define DOF_SECOND_CIRCLE_START 2.0f
#define DOF_SECOND_CIRCLE_FADE_LENGTH 1.0f
#define DOF_SECOND_CIRCLE_INV_FADE_LENGTH (1.0f/DOF_SECOND_CIRCLE_FADE_LENGTH)

#define DOF_THIRD_CIRCLE_START 4.0f
#define DOF_THIRD_CIRCLE_FADE_LENGTH 1.0f
#define DOF_THIRD_CIRCLE_INV_FADE_LENGTH (1.0f/DOF_THIRD_CIRCLE_FADE_LENGTH)

#define DOF_FOURTH_CIRCLE_START 8.0f
#define DOF_FOURTH_CIRCLE_FADE_LENGTH 1.0f
#define DOF_FOURTH_CIRCLE_INV_FADE_LENGTH (1.0f/DOF_FOURTH_CIRCLE_FADE_LENGTH)

static const float2 ConcentricTaps9[9] = { float2(0.0f, 0.0f), float2(0.4714045f, -0.4714045f), float2(-0.6666666f, 7.853982E-06f), 
float2(0.4714045f, 0.4714045f), float2(-7.838672E-06f, -0.6666666f), float2(-7.896953E-06f, 0.6666666f), float2(-0.4714045f, -0.4714045f), 
float2(0.6666666f, 7.853982E-06f), float2(-0.4714045f, 0.4714045f) };

static const float2 ConcentricTaps25[25] = { float2(0.0f, 0.0f), float2(0.5656855f, -0.5656855f), float2(-0.7391037f, -0.3061467f), float2(-0.8f, 7.853982E-06f), 
float2(-0.7391037f, 0.3061467f), float2(-0.5656855f, 0.5656855f), float2(0.3061468f, -0.7391036f), float2(0.2828427f, -0.2828427f), float2(-0.4f, 7.853982E-06f), 
float2(0.2828427f, 0.2828427f), float2(0.3061467f, 0.7391036f), float2(-7.785161E-06f, -0.8f), float2(-7.850328E-06f, -0.4f), float2(-7.885297E-06f, 0.4f), 
float2(-7.855097E-06f, 0.8f), float2(-0.3061467f, -0.7391036f), float2(-0.2828427f, -0.2828427f), float2(0.4f, 7.853982E-06f), float2(-0.2828427f, 0.2828427f), 
float2(-0.3061468f, 0.7391036f), float2(-0.5656854f, -0.5656855f), float2(0.7391036f, -0.3061467f), float2(0.8f, 7.853982E-06f), float2(0.7391036f, 0.3061467f), 
float2(-0.5656854f, 0.5656854f) }; 

//static const float2 ConcentricTaps49[49] = { float2(0.0f, 0.0f), float2(0.6060915f, -0.6060915f), float2(-0.7423075f, -0.4285714f), float2(-0.8279364f, -0.2218449f), 
//float2(-0.8571429f, 7.853982E-06f), float2(-0.8279364f, 0.2218449f), float2(-0.7423075f, 0.4285714f), float2(-0.6060916f, 0.6060915f), float2(0.4285715f, -0.7423075f), 
//float2(0.404061f, -0.404061f), float2(-0.5279311f, -0.2186763f), float2(-0.5714285f, 7.853982E-06f), float2(-0.5279311f, 0.2186763f), float2(0.404061f, 0.404061f), 
//float2(0.4285714f, 0.7423075f), float2(0.2218449f, -0.8279364f), float2(0.2186763f, -0.5279311f), float2(0.2020305f, -0.2020305f), float2(-0.2857143f, 7.853982E-06f), 
//float2(0.2020305f, 0.2020305f), float2(0.2186762f, 0.5279311f), float2(0.2218449f, 0.8279364f), float2(-7.830346E-06f, -0.8571429f), float2(-7.808775E-06f, -0.5714285f), 
//float2(-7.855323E-06f, -0.2857143f), float2(-7.880301E-06f, 0.2857143f), float2(-7.858731E-06f, 0.5714285f), float2(-7.90528E-06f, 0.8571428f), float2(-0.2218448f, -0.8279365f), 
//float2(-0.2186762f, -0.5279311f), float2(-0.2020305f, -0.2020305f), float2(0.2857143f, 7.853982E-06f), float2(-0.2020305f, 0.2020305f), float2(-0.2186763f, 0.5279311f), 
//float2(-0.221845f, 0.8279364f), float2(-0.4285714f, -0.7423075f), float2(-0.404061f, -0.404061f), float2(0.5279311f, -0.2186763f), float2(0.5714285f, 7.853982E-06f), 
//float2(0.5279311f, 0.2186763f), float2(-0.404061f, 0.404061f), float2(-0.4285715f, 0.7423074f), float2(-0.6060915f, -0.6060916f), float2(0.7423074f, -0.4285714f), 
//float2(0.8279364f, -0.2218449f), float2(0.8571428f, 7.853982E-06f), float2(0.8279364f, 0.2218449f), float2(0.7423074f, 0.4285714f), float2(-0.6060915f, 0.6060915f) }; 

static const float2 ConcentricTaps49[49] = { float2(0.0f, 0.0f), float2(0.2020305f, -0.2020305f), float2(-0.2857143f, 7.853982E-06f), float2(0.2020305f, 0.2020305f), 
float2(-7.855323E-06f, -0.2857143f), float2(-7.880301E-06f, 0.2857143f), float2(-0.2020305f, -0.2020305f), float2(0.2857143f, 7.853982E-06f), float2(-0.2020305f, 0.2020305f), 
float2(0.404061f, -0.404061f), float2(-0.5279311f, -0.2186763f), float2(-0.5714285f, 7.853982E-06f), float2(-0.5279311f, 0.2186763f), float2(0.404061f, 0.404061f), 
float2(0.2186763f, -0.5279311f), float2(0.2186762f, 0.5279311f), float2(-7.808775E-06f, -0.5714285f), float2(-7.858731E-06f, 0.5714285f), float2(-0.2186762f, -0.5279311f), 
float2(-0.2186763f, 0.5279311f), float2(-0.404061f, -0.404061f), float2(0.5279311f, -0.2186763f), float2(0.5714285f, 7.853982E-06f), float2(0.5279311f, 0.2186763f), 
float2(-0.404061f, 0.404061f), float2(0.6060915f, -0.6060915f), float2(-0.7423075f, -0.4285714f), float2(-0.8279364f, -0.2218449f), float2(-0.8571429f, 7.853982E-06f), 
float2(-0.8279364f, 0.2218449f), float2(-0.7423075f, 0.4285714f), float2(-0.6060916f, 0.6060915f), float2(0.4285715f, -0.7423075f), float2(0.4285714f, 0.7423075f), 
float2(0.2218449f, -0.8279364f), float2(0.2218449f, 0.8279364f), float2(-7.830346E-06f, -0.8571429f), float2(-7.90528E-06f, 0.8571428f), float2(-0.2218448f, -0.8279365f), 
float2(-0.221845f, 0.8279364f), float2(-0.4285714f, -0.7423075f), float2(-0.4285715f, 0.7423074f), float2(-0.6060915f, -0.6060916f), float2(0.7423074f, -0.4285714f), 
float2(0.8279364f, -0.2218449f), float2(0.8571428f, 7.853982E-06f), float2(0.8279364f, 0.2218449f), float2(0.7423074f, 0.4285714f), float2(-0.6060915f, 0.6060915f) }; 

static const float2 ConcentricTapsNormalized49[49] = { float2(0.0f, 0.0f), float2(0.7071068f, -0.7071068f), float2(-1.0f, 0.0f), float2(0.7071068f, 0.7071068f), 
float2(0.0f, -1.0f), float2(0.0f, 1.0f), float2(-0.7071068f, -0.7071068f), float2(1.0f, 0.0f), float2(-0.7071068f, 0.7071068f), 
float2(0.7071068f, -0.7071068f), float2(-0.9238794f, -0.3826835f), float2(-1.0f, 0.0f), float2(-0.9238794f, 0.3826835f), float2(0.7071068f, 0.7071068f), 
float2(0.3826835f, -0.9238794f), float2(0.3826834f, 0.9238794f), float2(0.0f, -1.0f), float2(0.0f, 1.0f), float2(-0.3826834f, -0.9238794f), 
float2(-0.3826835f, 0.9238794f), float2(-0.7071068f, -0.7071068f), float2(0.9238794f, -0.3826835f), float2(1.0f, 0.0f), float2(0.9238794f, 0.3826835f), 
float2(-0.7071068f, 0.7071068f), float2(0.7071068f, -0.7071068f), float2(-0.8660254f, -0.4999999f), float2(-0.9659259f, -0.258819f), float2(-1.0f, 0.0f), 
float2(-0.9659259f, 0.258819f), float2(-0.8660254f, 0.4999999f), float2(-0.7071068f, 0.7071068f), float2(0.5000001f, -0.8660253f), float2(0.5f, 0.8660254f), 
float2(0.258819f, -0.9659258f), float2(0.2588191f, 0.9659258f), float2(0.0f, -1.0f), float2(0.0f, 1.0f), float2(-0.2588189f, -0.9659258f), 
float2(-0.2588192f, 0.9659258f), float2(-0.4999999f, -0.8660254f), float2(-0.5000001f, 0.8660254f), float2(-0.7071068f, -0.7071068f), float2(0.8660254f, -0.5f), 
float2(0.9659258f, -0.258819f), float2(1.0f, 0.0f), float2(0.9659258f, 0.258819f), float2(0.8660254f, 0.5f), float2(-0.7071068f, 0.7071068f) }; 

static const float2 PoissonTaps49[49] = { float2(0.0f, 0.0f), float2(-0.3209127f, 0.06563285f), float2(-0.2775771f, -0.5298831f), float2(-0.59569f, -0.08887017f), 
float2(-0.1177111f, -0.1613725f), float2(-0.5932315f, -0.483787f), float2(-0.7775061f, -0.3123448f), float2(-0.1246743f, -0.376587f), float2(-0.5038848f, -0.7610875f), 
float2(-0.7816532f, -0.5605057f), float2(-0.6414767f, 0.1652961f), float2(-0.882744f, 0.1575853f), float2(0.2613454f, -0.123814f), float2(0.1974508f, -0.3582167f), 
float2(-0.0116339f, 0.1817625f), float2(0.07756674f, -0.02389619f), float2(0.2946354f, 0.3068724f), float2(-0.3013498f, 0.4119555f), float2(0.09937111f, 0.3794914f), 
float2(0.4396013f, 0.0606817f), float2(0.1498609f, -0.6223022f), float2(-0.0620923f, -0.7611532f), float2(-0.6875692f, 0.4921291f), float2(-0.9103686f, -0.07208584f), 
float2(-0.2973843f, -0.8334335f), float2(-0.3876573f, 0.7354805f), float2(-0.6698617f, 0.7099829f), float2(0.6071526f, 0.4062979f), float2(0.3239519f, 0.579985f), 
float2(0.6434115f, 0.1727395f), float2(-0.4951413f, 0.5559067f), float2(-0.1028288f, 0.5281211f), float2(-0.1944688f, 0.9450746f), float2(0.0002959371f, 0.7179604f), 
float2(0.5081258f, -0.2817014f), float2(0.760179f, -0.03851823f), float2(0.3753645f, -0.6347113f), float2(-0.03067559f, -0.9640083f), float2(0.234541f, -0.8366652f),
float2(0.5647001f, -0.8067597f), float2(0.6695956f, -0.4214241f), float2(0.5438291f, 0.6930436f), float2(0.3165668f, 0.8355474f), float2(0.1484994f, 0.9533116f), 
float2(0.8543869f, 0.1682153f), float2(0.9971664f, 0.01701772f), float2(0.9215452f, -0.2136744f), float2(0.9169838f, 0.3651171f), float2(0.7776117f, 0.5424775f) };

static const float2 PoissonTaps81[81] = { float2(0.0f, 0.0f), float2(-0.4559993f, 0.2711681f), float2(-0.2861495f, 0.4314983f), float2(-0.3009396f, 0.1480813f), 
float2(-0.6628067f, 0.3943727f), float2(-0.530942f, -0.04073346f), float2(-0.7079221f, 0.0921108f), float2(-0.6895049f, -0.1575469f), float2(-0.9592673f, -0.0875392f), 
float2(-0.9023319f, 0.1903859f), float2(-0.8535916f, 0.3501035f), float2(-0.1283677f, 0.2055954f), float2(-0.1811345f, -0.06593624f), float2(-0.4790154f, 0.4837705f), 
float2(-0.6316663f, 0.6470624f), float2(-0.3980838f, -0.1572448f), float2(-0.5484f, 0.8269801f), float2(-0.4221465f, 0.707813f), float2(0.002084315f, 0.5593213f), 
float2(-0.08918954f, 0.3696463f), float2(-0.2370599f, 0.6920133f), float2(-0.9035524f, -0.2939975f), float2(-0.641973f, -0.4062065f), float2(-0.833988f, -0.4539633f), 
float2(-0.5380228f, -0.274217f), float2(-0.2079146f, -0.2713028f), float2(-0.03500843f, 0.04022425f), float2(-0.01656269f, -0.2207742f), float2(-0.7771803f, 0.5138243f), 
float2(-0.3215073f, 0.8750488f), float2(-0.01707576f, -0.4226323f), float2(-0.2282565f, -0.5259559f), float2(0.2988932f, -0.1790867f), float2(0.1579546f, -0.3244246f), 
float2(0.1143715f, -0.05773541f), float2(0.26343f, 0.05042776f), float2(0.1432841f, 0.176945f), float2(0.4257133f, 0.011085f), float2(-0.3436857f, -0.3810338f), 
float2(0.582574f, -0.04776183f), float2(0.74879f, -0.03524271f), float2(0.5409728f, -0.243167f), float2(0.5601217f, 0.1554045f), float2(0.4131866f, 0.2867268f), 
float2(0.8874826f, -0.1532465f), float2(0.7905437f, 0.1501322f), float2(0.7284622f, -0.2117865f), float2(0.7474062f, -0.4968464f), float2(0.9228518f, 0.008407742f), 
float2(0.8924621f, -0.3278955f), float2(-0.6035599f, -0.5876982f), float2(0.5219095f, -0.4400916f), float2(0.3427274f, -0.3385582f), float2(-0.4950714f, -0.7144395f), 
float2(-0.4456085f, -0.5250954f), float2(-0.2861098f, -0.69499f), float2(-0.259123f, -0.9182754f), float2(0.2893499f, 0.4714047f), float2(0.5986701f, 0.5371037f), 
float2(0.6807226f, 0.2799722f), float2(0.277942f, -0.4998321f), float2(0.6397538f, -0.6668575f), float2(-0.1303706f, 0.913931f), float2(-0.01048252f, 0.7770689f), 
float2(0.3799788f, 0.7468764f), float2(0.1876511f, 0.7772338f), float2(-0.1246857f, -0.771849f), float2(0.06615439f, -0.6613742f), float2(0.3568611f, -0.7123691f), 
float2(0.103426f, -0.8370228f), float2(0.4925349f, -0.8376436f), float2(0.05175085f, 0.963848f), float2(0.292669f, 0.9047402f), float2(0.1100849f, 0.3735225f), 
float2(-0.07890929f, -0.9682799f), float2(0.897827f, 0.317976f), float2(0.173922f, 0.6037411f), float2(0.2517254f, -0.9169639f), float2(0.7623788f, 0.4369915f), 
float2(0.5624475f, 0.7236511f), float2(0.9769022f, 0.1683603f) };

//static const float2 ConcentricTaps81[81] = { float2(0.0f, 0.0f), float2(0.6285394f, -0.6285394f), float2(-0.7390841f, -0.4938402f), float2(-0.8212263f, -0.3401631f), 
//float2(-0.8718092f, -0.1734136f), float2(-0.8888889f, 7.853982E-06f), float2(-0.8718092f, 0.1734136f), float2(-0.8212263f, 0.3401631f), float2(-0.7390841f, 0.4938402f), 
//float2(-0.6285394f, 0.6285394f), float2(0.4938402f, -0.7390841f), float2(0.4714045f, -0.4714045f), float2(-0.5773502f, -0.3333333f), float2(-0.6439505f, -0.172546f), 
//float2(-0.6666666f, 7.853982E-06f), float2(-0.6439505f, 0.172546f), float2(-0.5773502f, 0.3333333f), float2(0.4714045f, 0.4714045f), float2(0.4938401f, 0.7390841f), 
//float2(0.3401631f, -0.8212262f), float2(0.3333333f, -0.5773502f), float2(0.3142697f, -0.3142697f), float2(-0.4106131f, -0.1700815f), float2(-0.4444444f, 7.853982E-06f), 
//float2(-0.4106131f, 0.1700815f), float2(0.3142697f, 0.3142697f), float2(0.3333333f, 0.5773503f), float2(0.340163f, 0.8212262f), float2(0.1734136f, -0.8718091f), 
//float2(0.1725461f, -0.6439505f), float2(0.1700816f, -0.4106131f), float2(0.1571348f, -0.1571348f), float2(-0.2222222f, 7.853982E-06f), float2(0.1571348f, 0.1571348f), 
//float2(0.1700815f, 0.4106131f), float2(0.172546f, 0.6439505f), float2(0.1734135f, 0.8718091f), float2(-7.802468E-06f, -0.8888889f), float2(-7.838672E-06f, -0.6666666f),
//float2(-7.821894E-06f, -0.4444444f), float2(-7.831608E-06f, -0.2222222f), float2(-7.851035E-06f, 0.2222222f), float2(-7.860749E-06f, 0.4444444f), 
//float2(-7.896953E-06f, 0.6666666f), float2(-7.880176E-06f, 0.8888888f), float2(-0.1734135f, -0.8718092f), float2(-0.172546f, -0.6439505f), float2(-0.1700815f, -0.4106131f), 
//float2(-0.1571348f, -0.1571348f), float2(0.2222222f, 7.853982E-06f), float2(-0.1571348f, 0.1571348f), float2(-0.1700816f, 0.4106131f), float2(-0.1725461f, 0.6439505f), 
//float2(-0.1734136f, 0.8718091f), float2(-0.3401631f, -0.8212262f), float2(-0.3333333f, -0.5773503f), float2(-0.3142697f, -0.3142697f), float2(0.4106131f, -0.1700815f), 
//float2(0.4444444f, 7.853982E-06f), float2(0.4106131f, 0.1700815f), float2(-0.3142697f, 0.3142697f), float2(-0.3333333f, 0.5773502f), float2(-0.3401631f, 0.8212262f),
//float2(-0.4938402f, -0.7390841f), float2(-0.4714045f, -0.4714045f), float2(0.5773502f, -0.3333333f), float2(0.6439505f, -0.172546f), float2(0.6666666f, 7.853982E-06f), 
//float2(0.6439505f, 0.172546f), float2(0.5773502f, 0.3333333f), float2(-0.4714045f, 0.4714045f), float2(-0.4938403f, 0.739084f), float2(-0.6285393f, -0.6285394f), 
//float2(0.7390841f, -0.4938402f), float2(0.8212262f, -0.3401631f), float2(0.8718091f, -0.1734136f), float2(0.8888888f, 7.853982E-06f), float2(0.8718091f, 0.1734136f), 
//float2(0.8212262f, 0.3401631f), float2(0.7390841f, 0.4938402f), float2(-0.6285393f, 0.6285393f) }; 

static const float2 ConcentricTaps81[81] = { float2(0.0f, 0.0f), float2(0.1571348f, -0.1571348f), float2(-0.2222222f, 7.853982E-06f), float2(0.1571348f, 0.1571348f), 
float2(-7.831608E-06f, -0.2222222f), float2(-7.851035E-06f, 0.2222222f), float2(-0.1571348f, -0.1571348f), float2(0.2222222f, 7.853982E-06f), float2(-0.1571348f, 0.1571348f), 
float2(0.3142697f, -0.3142697f), float2(-0.4106131f, -0.1700815f), float2(-0.4444444f, 7.853982E-06f), float2(-0.4106131f, 0.1700815f), float2(0.3142697f, 0.3142697f), 
float2(0.1700816f, -0.4106131f), float2(0.1700815f, 0.4106131f), float2(-7.821894E-06f, -0.4444444f), float2(-7.860749E-06f, 0.4444444f), float2(-0.1700815f, -0.4106131f), 
float2(-0.1700816f, 0.4106131f), float2(-0.3142697f, -0.3142697f), float2(0.4106131f, -0.1700815f), float2(0.4444444f, 7.853982E-06f), float2(0.4106131f, 0.1700815f), 
float2(-0.3142697f, 0.3142697f), float2(0.4714045f, -0.4714045f), float2(-0.5773502f, -0.3333333f), float2(-0.6439505f, -0.172546f), float2(-0.6666666f, 7.853982E-06f), 
float2(-0.6439505f, 0.172546f), float2(-0.5773502f, 0.3333333f), float2(0.4714045f, 0.4714045f), float2(0.3333333f, -0.5773502f), float2(0.3333333f, 0.5773503f), 
float2(0.1725461f, -0.6439505f), float2(0.172546f, 0.6439505f), float2(-7.838672E-06f, -0.6666666f), float2(-7.896953E-06f, 0.6666666f), float2(-0.172546f, -0.6439505f), 
float2(-0.1725461f, 0.6439505f), float2(-0.3333333f, -0.5773503f), float2(-0.3333333f, 0.5773502f), float2(-0.4714045f, -0.4714045f), float2(0.5773502f, -0.3333333f), 
float2(0.6439505f, -0.172546f), float2(0.6666666f, 7.853982E-06f), float2(0.6439505f, 0.172546f), float2(0.5773502f, 0.3333333f), float2(-0.4714045f, 0.4714045f), 
float2(0.6285394f, -0.6285394f), float2(-0.7390841f, -0.4938402f), float2(-0.8212263f, -0.3401631f), float2(-0.8718092f, -0.1734136f), float2(-0.8888889f, 7.853982E-06f), 
float2(-0.8718092f, 0.1734136f), float2(-0.8212263f, 0.3401631f), float2(-0.7390841f, 0.4938402f), float2(-0.6285394f, 0.6285394f), float2(0.4938402f, -0.7390841f), 
float2(0.4938401f, 0.7390841f), float2(0.3401631f, -0.8212262f), float2(0.340163f, 0.8212262f), float2(0.1734136f, -0.8718091f), float2(0.1734135f, 0.8718091f), 
float2(-7.802468E-06f, -0.8888889f), float2(-7.880176E-06f, 0.8888888f), float2(-0.1734135f, -0.8718092f), float2(-0.1734136f, 0.8718091f), float2(-0.3401631f, -0.8212262f), 
float2(-0.3401631f, 0.8212262f), float2(-0.4938402f, -0.7390841f), float2(-0.4938403f, 0.739084f), float2(-0.6285393f, -0.6285394f), float2(0.7390841f, -0.4938402f), 
float2(0.8212262f, -0.3401631f), float2(0.8718091f, -0.1734136f), float2(0.8888888f, 7.853982E-06f), float2(0.8718091f, 0.1734136f), float2(0.8212262f, 0.3401631f), 
float2(0.7390841f, 0.4938402f), float2(-0.6285393f, 0.6285393f) }; 

static const float2 ConcentricTaps169[169] = { float2(0.0f, 0.0f), float2(0.652714f, -0.652714f), float2(-0.7323261f, -0.5619337f), float2(-0.7994081f, -0.4615384f), 
float2(-0.8528119f, -0.3532462f), float2(-0.8916239f, -0.2389099f), float2(-0.9151799f, -0.1204857f), float2(-0.9230769f, 7.853982E-06f), float2(-0.9151799f, 0.1204857f), 
float2(-0.8916239f, 0.2389099f), float2(-0.8528119f, 0.3532462f), float2(-0.7994081f, 0.4615384f), float2(-0.7323261f, 0.5619336f), float2(-0.652714f, 0.652714f), 
float2(0.5619336f, -0.7323261f), float2(0.5439283f, -0.5439283f), float2(-0.6223208f, -0.4521425f), float2(-0.6853896f, -0.3492235f), float2(-0.731582f, -0.2377054f), 
float2(-0.7597603f, -0.1203342f), float2(-0.7692308f, 7.853982E-06f), float2(-0.7597603f, 0.1203342f), float2(-0.731582f, 0.2377054f), float2(-0.6853896f, 0.3492235f), 
float2(-0.6223208f, 0.4521425f), float2(-0.5439283f, 0.5439283f), float2(0.5619336f, 0.7323261f), float2(0.4615385f, -0.7994081f), float2(0.4521424f, -0.6223208f), 
float2(0.4351426f, -0.4351426f), float2(-0.5116736f, -0.3418894f), float2(-0.5685412f, -0.2354975f), float2(-0.6035601f, -0.1200556f), float2(-0.6153846f, 7.853982E-06f), 
float2(-0.6035601f, 0.1200556f), float2(-0.5685412f, 0.2354975f), float2(-0.5116736f, 0.3418894f), float2(0.4351426f, 0.4351426f), float2(0.4521425f, 0.6223207f), 
float2(0.4615384f, 0.7994081f), float2(0.3532463f, -0.8528119f), float2(0.3492236f, -0.6853896f), float2(0.3418894f, -0.5116735f), float2(0.3263569f, -0.3263569f), 
float2(-0.399704f, -0.2307692f), float2(-0.4458119f, -0.1194549f), float2(-0.4615384f, 7.853982E-06f), float2(-0.4458119f, 0.1194549f), float2(-0.399704f, 0.2307692f), 
float2(0.3263569f, 0.3263569f), float2(0.3418893f, 0.5116736f), float2(0.3492235f, 0.6853896f), float2(0.3532462f, 0.8528118f), float2(0.2389099f, -0.8916239f), 
float2(0.2377054f, -0.7315819f), float2(0.2354975f, -0.5685412f), float2(0.2307692f, -0.399704f), float2(0.2175713f, -0.2175713f), float2(-0.2842706f, -0.1177488f), 
float2(-0.3076923f, 7.853982E-06f), float2(-0.2842706f, 0.1177488f), float2(0.2175713f, 0.2175713f), float2(0.2307692f, 0.399704f), float2(0.2354975f, 0.5685412f), 
float2(0.2377053f, 0.7315819f), float2(0.2389099f, 0.8916238f), float2(0.1204857f, -0.9151799f), float2(0.1203342f, -0.7597603f), float2(0.1200556f, -0.6035601f), 
float2(0.119455f, -0.4458119f), float2(0.1177488f, -0.2842706f), float2(0.1087857f, -0.1087857f), float2(-0.1538461f, 7.853982E-06f), float2(0.1087857f, 0.1087857f), 
float2(0.1177487f, 0.2842706f), float2(0.119455f, 0.4458119f), float2(0.1200555f, 0.6035601f), float2(0.1203341f, 0.7597603f), float2(0.1204856f, 0.9151798f), 
float2(-7.772444E-06f, -0.9230769f), float2(-7.852529E-06f, -0.7692308f), float2(-7.822574E-06f, -0.6153846f), float2(-7.847638E-06f, -0.4615384f), 
float2(-7.836023E-06f, -0.3076923f), float2(-7.842748E-06f, -0.1538461f), float2(-7.856197E-06f, 0.1538461f), float2(-7.862923E-06f, 0.3076923f), 
float2(-7.887988E-06f, 0.4615384f), float2(-7.876372E-06f, 0.6153846f), float2(-7.919776E-06f, 0.7692307f), float2(-7.853142E-06f, 0.9230769f), 
float2(-0.1204857f, -0.9151799f), float2(-0.1203341f, -0.7597603f), float2(-0.1200555f, -0.6035601f), float2(-0.119455f, -0.4458119f), float2(-0.1177487f, -0.2842706f), 
float2(-0.1087857f, -0.1087857f), float2(0.1538461f, 7.853982E-06f), float2(-0.1087857f, 0.1087857f), float2(-0.1177488f, 0.2842706f), float2(-0.119455f, 0.4458119f), 
float2(-0.1200556f, 0.6035601f), float2(-0.1203342f, 0.7597603f), float2(-0.1204857f, 0.9151798f), float2(-0.2389098f, -0.8916239f), float2(-0.2377054f, -0.731582f), 
float2(-0.2354975f, -0.5685412f), float2(-0.2307692f, -0.399704f), float2(-0.2175713f, -0.2175713f), float2(0.2842706f, -0.1177488f), float2(0.3076923f, 7.853982E-06f), 
float2(0.2842706f, 0.1177488f), float2(-0.2175713f, 0.2175713f), float2(-0.2307692f, 0.399704f), float2(-0.2354975f, 0.5685412f), float2(-0.2377054f, 0.7315819f), 
float2(-0.23891f, 0.8916238f), float2(-0.3532462f, -0.8528119f), float2(-0.3492234f, -0.6853897f), float2(-0.3418893f, -0.5116736f), float2(-0.3263569f, -0.3263569f), 
float2(0.399704f, -0.2307692f), float2(0.4458119f, -0.1194549f), float2(0.4615384f, 7.853982E-06f), float2(0.4458119f, 0.1194549f), float2(0.399704f, 0.2307692f), 
float2(-0.3263569f, 0.3263569f), float2(-0.3418894f, 0.5116735f), float2(-0.3492235f, 0.6853896f), float2(-0.3532463f, 0.8528118f), float2(-0.4615384f, -0.7994081f), 
float2(-0.4521424f, -0.6223208f), float2(-0.4351426f, -0.4351426f), float2(0.5116736f, -0.3418894f), float2(0.5685412f, -0.2354975f), float2(0.6035601f, -0.1200556f), 
float2(0.6153846f, 7.853982E-06f), float2(0.6035601f, 0.1200556f), float2(0.5685412f, 0.2354975f), float2(0.5116736f, 0.3418894f), float2(-0.4351426f, 0.4351426f), 
float2(-0.4521424f, 0.6223207f), float2(-0.4615385f, 0.799408f), float2(-0.5619336f, -0.7323262f), float2(-0.5439283f, -0.5439283f), float2(0.6223207f, -0.4521425f), 
float2(0.6853896f, -0.3492235f), float2(0.7315819f, -0.2377054f), float2(0.7597603f, -0.1203342f), float2(0.7692307f, 7.853982E-06f), float2(0.7597603f, 0.1203342f), 
float2(0.7315819f, 0.2377054f), float2(0.6853896f, 0.3492235f), float2(0.6223207f, 0.4521425f), float2(-0.5439283f, 0.5439283f), float2(-0.5619336f, 0.7323261f), 
float2(-0.6527139f, -0.652714f), float2(0.7323261f, -0.5619336f), float2(0.799408f, -0.4615384f), float2(0.8528118f, -0.3532462f), float2(0.8916238f, -0.2389099f), 
float2(0.9151798f, -0.1204857f), float2(0.9230769f, 7.853982E-06f), float2(0.9151798f, 0.1204857f), float2(0.8916238f, 0.2389099f), float2(0.8528118f, 0.3532462f), 
float2(0.799408f, 0.4615384f), float2(0.7323261f, 0.5619336f), float2(-0.6527139f, 0.6527139f) }; 

static const float2 ConcentricTapsPentagon49[49] = { float2(0.0f, 0.0f), float2(0.5861275f, -0.5783641f), float2(-0.729998f, -0.4279785f), float2(-0.7979411f, -0.2195195f), 
float2(-0.8215557f, -0.005469591f), float2(-0.800225f, 0.2087119f), float2(-0.7345075f, 0.4175641f), float2(-0.5988626f, 0.5909305f), float2(0.4228533f, -0.7212558f), 
float2(0.3907517f, -0.3855761f), float2(-0.5129526f, -0.2164895f), float2(-0.5477039f, -0.003643885f), float2(-0.515224f, 0.2093997f), float2(0.3934186f, 0.3986995f), 
float2(0.4077172f, 0.7171866f), float2(0.2259254f, -0.8212264f), float2(0.2196911f, -0.5205384f), float2(0.1953758f, -0.192788f), float2(-0.2738519f, -0.001818179f), 
float2(0.1967093f, 0.1993498f), float2(0.2063294f, 0.5076698f), float2(0.2074152f, 0.795253f), float2(0.005527173f, -0.8302022f), float2(0.003682274f, -0.553468f), 
float2(0.001837308f, -0.2767337f), float2(-0.001852556f, 0.2767347f), float2(-0.003697456f, 0.5534689f), float2(-0.005542421f, 0.830203f), float2(-0.2074151f, -0.7952532f), 
float2(-0.2063294f, -0.5076698f), float2(-0.1967093f, -0.1993498f), float2(0.2738518f, 0.001833234f), float2(-0.1953758f, 0.192788f), float2(-0.2196911f, 0.5205384f), 
float2(-0.2259255f, 0.8212262f), float2(-0.4077172f, -0.7171866f), float2(-0.3934186f, -0.3986995f), float2(0.515224f, -0.2093997f), float2(0.5477037f, 0.003658941f), 
float2(0.5129526f, 0.2164895f), float2(-0.3907517f, 0.3855761f), float2(-0.4228533f, 0.7212557f), float2(-0.590128f, -0.5980493f), float2(0.7345074f, -0.4175641f), 
float2(0.800225f, -0.2087119f), float2(0.8215557f, 0.005484648f), float2(0.7979411f, 0.2195195f), float2(0.7299979f, 0.4279785f), float2(-0.5861275f, 0.5783641f) }; 

uniform float _TemporalUVOffset;
float2 ShiftUV(float2 uv, float iter)
{
	float step1 = 1.37f * iter + _TemporalUVOffset;
	return uv * HalfResSize * step1;
}

void DepthOfFieldFirstTapIteration(float3 centerLighting, float centerDepth, float2 uv, inout float4 background, inout float4 foreground, float focalDistance)
{			
	#ifdef USING_PRESORT
	float3 presort = SamplePresortTexture(uv);
	float2 depthWeights = presort.yz;
	#else
	//Not implemented, compilation failure
	#endif
	
	float isNearFieldPixel = saturate(focalDistance - centerDepth);	
	float foregroundWeight	= (depthWeights.x) * (isNearFieldPixel + 1.0f) + 1e-4f;
	float backgroundWeight	= (depthWeights.y) + 1e-4f;	
	
	foreground 	= float4(centerLighting * foregroundWeight, foregroundWeight);
	background 	= float4(centerLighting * backgroundWeight, backgroundWeight);
}

void DepthOfFieldTapIteration(int k, float centerDepth, float2 inputUV, float2 sampleOffset, inout float4 background, 
								inout float4 foreground, float focalDistance, float sampleMultiplier)
{		
	float2 uv = inputUV + sampleOffset * InvHalfResSize; 
	
	float4 sampleLightingDepth 	= tex2Dlod(_HalfResSourceDepthTexture, float4(uv, 0.0f, 0.0f));
	float3 sampleLighting 		= sampleLightingDepth.xyz;
	float sampleDepth 			= sampleLightingDepth.w;	
	
	#ifdef USING_PRESORT
	float3 presort 		= SamplePresortTexture(uv);
	float clampedCoC	= presort.x;	
	float2 depthWeights = presort.yz;
	#else	
	float4 tiledNeighbourhood 	= tex2Dlod(_TiledNeighbourhoodData, float4(uv, 0.0f, 0.0f));	
	float neighborhoodMinDepth 	= tiledNeighbourhood.y;		
	float sampleCoC 			= CoCFromDepthSigned(sampleDepth, focalDistance);
	float clampedCoC			= clamp(abs(sampleCoC), 0.5f, MAX_COC_RADIUS);	
	float commonWeight 			= 1.0f / (clampedCoC*clampedCoC*3.1415f);	
	float2 depthWeights 		= DepthComparison(sampleDepth, neighborhoodMinDepth) * commonWeight;	
	#endif
	
	//Intersection test 
	float dist 				= length(sampleOffset);
	float intersection		= saturate((clampedCoC - dist + 0.25f) * 4.0f);
	//float intersection 		= dist < clampedCoC ? 1.0f : 0.0f;	
	
	#if 0
	intersection = 1;
	depthWeights = float2(0,1);
	#endif
	
	float commonWeight 		= intersection * sampleMultiplier;
	
	float isNearFieldPixel = saturate((focalDistance - sampleDepth));	
	float foregroundWeight	= commonWeight * (depthWeights.x) * (isNearFieldPixel + 1.0f) + 1e-3f;
	float backgroundWeight	= commonWeight * (depthWeights.y) + 1e-3f;
	
	float4 foregroundAddition 	= float4(sampleLighting * foregroundWeight, foregroundWeight);
	float4 backgroundAddition 	= float4(sampleLighting * backgroundWeight, backgroundWeight);
	
	#ifdef SC_DOF_MASK_ON
	//Compiler should flatten this 
	float sampleValidity = tex2Dlod(_ExclusionMask, float4(uv, 0.0f, 0.0f)).x;	
	foregroundAddition	*= sampleValidity;
	backgroundAddition 	*= sampleValidity;
	#endif
	
	foreground += foregroundAddition;
	background += backgroundAddition;
}

float3 DivideByAlpha(float4 value)
{
	return value.xyz / (value.w+1e-8f);
}	

void NormalizeIntegration(float numSamples, float searchRadius, inout float4 background, inout float4 foreground)
{
	float normFactor = (1.0f / numSamples) * (1.0f / InvCircleAreaClamped(searchRadius));
	background 	*= normFactor;
	foreground 	*= normFactor;
}

//Returns foreground alpha as .w component
float4 CombineDepthOfFieldLayers(inout float4 background, inout float4 foreground, float numSamples, float searchRadius)
{	
	float normFactor = (1.0f / numSamples) * (1.0f / InvCircleAreaClamped(searchRadius));		
	float alpha = saturate(foreground.w * normFactor);
		
	//return (background).wwww;
	//return (foreground).wwww;
	//return (foreground).xyzz;
	//return (background).xyzz;
	//return (alpha).xxxx;
	//return DivideByAlpha(foreground).xyzz;
	//return DivideByAlpha(background).xyzz;
	//return source;
	
	float3 result = lerp(DivideByAlpha(background), DivideByAlpha(foreground), alpha);	
	return float4(result, alpha);
}

float DepthOfFieldAlpha(float foregroundAlpha, float centerDepth, float centerCoC, float focalDistance)
{		
	//float alpha = saturate(centerCoC*0.5f + foregroundAlpha*2.0f);
	//float alpha = saturate(centerCoC*0.05f);
	float alpha = saturate(foregroundAlpha*0.1f);
	return alpha;
}		

void DepthOfFieldTaps(float2 inputUV, float searchRadius, float focalDistance, float centerDepth, float2 offsetArray[TAPS_ARRAY_SIZE], 
						int kStart, int kEnd, inout float4 background, inout float4 foreground, float randomOffsetMultiplier, float sampleMultiplier)
{								
	for (int k = kStart; k < kEnd; k++)  
	{			
		float2 randomOffset = RandomOffset(ShiftUV(inputUV, k));
		float2 sampleOffset = offsetArray[k] + randomOffset * randomOffsetMultiplier;		
		sampleOffset = sampleOffset * searchRadius;
																
		DepthOfFieldTapIteration(k, centerDepth, inputUV, sampleOffset, background, foreground, focalDistance, sampleMultiplier);
	}
}

float3 NormalizeColor(float3 clr)
{
	return clr / VectorMax(clr);
}

float ColorBasedWeightMetric(float3 clr1, float3 clr2)
{
	clr1 = NormalizeColor(clr1);
	clr2 = NormalizeColor(clr2);
	
	float3 diff = abs(clr1 - clr2);
	return 1.0f - saturate(dot(diff, diff) * 0.5f);
}

BlurTapOutput PackBlurOutput(float3 clr, float depth, float alpha)
{
	BlurTapOutput output;
	output.colorAndDepth = float4(clr, depth);
	output.alpha = alpha.xxxx;
	return output;
}			
									
BlurTapOutput BlurTapPass(v2f i)
{		
	#if 0
	BlurTapOutput output;
	output.colorAndDepth = tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f));
	output.alpha = 1.0f;
	return output;
	#endif
	
	float4 tiledNeighbourhood 	= tex2Dlod(_TiledNeighbourhoodData, float4(i.uv, 0.0f, 0.0f));	
	float searchRadius 			= tiledNeighbourhood.x; //Neighbourhood max CoC
	float focalDistance 		= GetFocalDistance();
	
	float4 centerSource 	= tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f));
	float3 centerLighting 	= centerSource.xyz;
	float centerDepth		= centerSource.w;
	
	//return PackBlurOutput(tiledNeighbourhood.yyy * 0.1f, centerDepth, 0.0f);	
	//return PackBlurOutput(float3(DepthComparison(centerDepth, tiledNeighbourhood.y), 0.0f), centerDepth, 0.0f);	
	//return PackBlurOutput(CoCFromDepth(centerDepth, focalDistance).xxx, centerDepth, 0.0f);
	//return PackBlurOutput(float3(depthWeights, 0.0f), centerDepth, 0.0f);	
	//return PackBlurOutput(searchRadius.xxx * 0.2f, centerDepth, 0.0f);	
	//return PackBlurOutput(centerLighting, centerDepth, 0.0f);
	//return PackBlurOutput(centerDepth.xxx * 0.01f, centerDepth, 0.0f);
	//return PackBlurOutput(tex2Dlod(_ExclusionMask, float4(i.uv, 0.0f, 0.0f)).xxx, centerDepth, 0.0f);	
	//return PackBlurOutput(float3(1,1,0), centerDepth, 0.0f);10.0f, centerDepth, 0.0f);	
		
	#ifdef SC_DOF_MASK_ON	
	//This is either 0 or 1, depending on if the object is excluded or not
	float sampleValidity 	= tex2Dlod(_ExclusionMask, float4(i.uv, 0.0f, 0.0f)).x;
	searchRadius 			= searchRadius * sampleValidity;
	#endif
	
	#ifdef USE_SINGLE_TAP_EARLY_OUT
	SCION_BRANCH if (searchRadius < SINGLE_TAP_EARLY_OUT_PIXEL_RADIUS)
	{
		#ifdef DEBUG_OPTIMIZATIONS
	 	centerLighting = centerLighting * float3(1.0f,0.3f,0.3f);
		#endif
		return PackBlurOutput(centerLighting, centerDepth, 0.0f);
	}
	#endif
		
	float4 background 	= 0.0f;
	float4 foreground 	= 0.0f;	 
		
	DepthOfFieldFirstTapIteration(centerLighting, centerDepth, i.uv, background, foreground, focalDistance);
	
	#ifdef SCION_DOF_49_SAMPLES
	const float randomOffsetMultiplier = 0.25f;
	const float radiusMult1 = 0.2857143f;
	const float radiusMult2 = 0.5714285f;
	const float radiusMult3 = 0.8571429f;
	
		#ifdef USE_REDUCED_SAMPLES_OPTIMIZATION
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult1, focalDistance, centerDepth, TAPS_ARRAY, 1, 9, background, foreground, randomOffsetMultiplier, 1.0f);	
		
		SCION_BRANCH if (searchRadius > DOF_SECOND_CIRCLE_START)
		{
			float secondCircleFadeIn = saturate(searchRadius - DOF_SECOND_CIRCLE_START);
			DepthOfFieldTaps(i.uv, searchRadius * radiusMult2, focalDistance, centerDepth, TAPS_ARRAY, 9, 25, background, foreground, randomOffsetMultiplier, secondCircleFadeIn);	
			
			SCION_BRANCH if (searchRadius > DOF_THIRD_CIRCLE_START)
			{
				float thirdCircleFadeIn = saturate(searchRadius - DOF_THIRD_CIRCLE_START);
				DepthOfFieldTaps(i.uv, searchRadius * radiusMult3, focalDistance, centerDepth, TAPS_ARRAY, 25, 49, background, foreground, randomOffsetMultiplier, thirdCircleFadeIn);	
			}
		} 
		#else
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult1, focalDistance, centerDepth, TAPS_ARRAY, 1, 9, background, foreground, randomOffsetMultiplier, 1);	
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult2, focalDistance, centerDepth, TAPS_ARRAY, 9, 25, background, foreground, randomOffsetMultiplier, 1);	
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult3, focalDistance, centerDepth, TAPS_ARRAY, 25, 49, background, foreground, randomOffsetMultiplier, 1);	
		#endif
	float4 depthOfFieldTaps = CombineDepthOfFieldLayers(background, foreground, 49, searchRadius);	
	#endif //SCION_DOF_49_SAMPLES
	
	#ifdef SCION_DOF_25_SAMPLES
	const float randomOffsetMultiplier = 0.35f;
	const float radiusMult1 = 0.40f;
	const float radiusMult2 = 0.80f;
	
		#ifdef USE_REDUCED_SAMPLES_OPTIMIZATION
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult1, focalDistance, centerDepth, TAPS_ARRAY, 1, 9, background, foreground, randomOffsetMultiplier, 1.0f);	
		
		SCION_BRANCH if (searchRadius > DOF_SECOND_CIRCLE_START)
		{
			float secondCircleFadeIn = saturate(searchRadius - DOF_SECOND_CIRCLE_START);
			DepthOfFieldTaps(i.uv, searchRadius * radiusMult2, focalDistance, centerDepth, TAPS_ARRAY, 9, 25, background, foreground, randomOffsetMultiplier, secondCircleFadeIn);	
		} 
		#else
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult1, focalDistance, centerDepth, TAPS_ARRAY, 1, 9, background, foreground, randomOffsetMultiplier, 1);	
		DepthOfFieldTaps(i.uv, searchRadius * radiusMult2, focalDistance, centerDepth, TAPS_ARRAY, 9, 25, background, foreground, randomOffsetMultiplier, 1);	
		#endif
	float4 depthOfFieldTaps = CombineDepthOfFieldLayers(background, foreground, 25, searchRadius);	
	#endif //SCION_DOF_25_SAMPLES
		
	return PackBlurOutput(depthOfFieldTaps.xyz, centerDepth, depthOfFieldTaps.w);
}
