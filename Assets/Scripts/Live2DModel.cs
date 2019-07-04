using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using live2d;
using live2d.framework;

public class Live2DModel : MonoBehaviour {

    public TextAsset modelTxt;    //模型文件
    public Texture2D[] texture2d; //模型贴图
    public TextAsset[] motionTxt; //模型动作文件
    public TextAsset[] expressionTxt; //表情文件
    

    private Live2DModelUnity live2dModel; //模型对象
    private Live2DMotion[] motions;     //模型动作对象
    private Matrix4x4 live2dCanvasPos;    //模型画布位置
    private MotionQueueManager motionQueueManager; //模型动作管理器
    private L2DMotionManager l2dMotionManager; //模型动作管理器
    private EyeBlinkMotion eyeBlinkMotion; //自动眨眼动作
    private L2DTargetPoint drag;    //拖拽动作
    private PhysicsHair physicHairSide; //侧边头发物理
    private PhysicsHair physicHairBack; //后侧头发物理
    private L2DExpressionMotion[] expressionMotions; //表情动作文件


    private int motionIndex = 0;

	void Start () {
        //初始化环境
        Live2D.init();
        //string path = Application.dataPath + "/Resources/Epsilon/runtime/Epsilon.moc";
        //Live2DModelUnity.loadModel(path);
        //加载模型文件
        live2dModel = Live2DModelUnity.loadModel(modelTxt.bytes);
        for (int i = 0; i < texture2d.Length; i++)
            live2dModel.setTexture(i, texture2d[i]);
        float modelWidth = live2dModel.getCanvasWidth();
        live2dCanvasPos = Matrix4x4.Ortho(0, modelWidth, modelWidth, 0, -50, 50);

        //加载模型动作文件
        motions = new Live2DMotion[motionTxt.Length];
        for (int i = 0; i < motions.Length; i++)
            motions[i] = Live2DMotion.loadMotion(motionTxt[i].bytes);
        l2dMotionManager = new L2DMotionManager();

        expressionMotions = new L2DExpressionMotion[expressionTxt.Length];
        for (int i = 0; i < expressionMotions.Length; i++)
            expressionMotions[i] = L2DExpressionMotion.loadJson(expressionTxt[i].bytes);

        eyeBlinkMotion = new EyeBlinkMotion();
        eyeBlinkMotion.setParam(live2dModel);
        drag = new L2DTargetPoint();
        physicHairBack = new PhysicsHair();
        physicHairSide = new PhysicsHair();
        physicHairSide.setup(0.2f, 0.5f, 0.14f);
        PhysicsHair.Src srcX = PhysicsHair.Src.SRC_TO_X; //横向摇摆
        physicHairSide.addSrcParam(srcX, "PARAM_ANGLE_X", 0.005f, 1);
        PhysicsHair.Target target = PhysicsHair.Target.TARGET_FROM_ANGLE; //表现形式
        physicHairSide.addTargetParam(target, "PARAM_HAIR_SIDE_L", 0.005f, 1);

        //motions[0].setLoop(true);
        motionQueueManager = new MotionQueueManager();
        //motionQueueManager.startMotion(motions[0]);
    }
	
	void Update () {
        live2dModel.setMatrix(transform.localToWorldMatrix * live2dCanvasPos);
        if (l2dMotionManager.isFinished())
            StartMotion(0, 1);
        else if (Input.GetKeyDown(KeyCode.Space))
            StartMotion(14, 2);
        l2dMotionManager.updateParam(live2dModel);

        Vector3 pos = Input.mousePosition;
        drag.Set(pos.x / Screen.width * 2 - 1, pos.y / Screen.height * 2 - 1);
        live2dModel.setParamFloat("PARAM_ANGLE_X", 30 * drag.getX());
        live2dModel.setParamFloat("PARAM_BODY_ANGLE_X", 10 * drag.getX());
        live2dModel.setParamFloat("PARAM_EYE_BALL_X", -drag.getX());
        live2dModel.setParamFloat("PARAM_EYE_BALL_Y", -drag.getY());
        drag.update();

        long time = UtSystem.getUserTimeMSec();
        physicHairSide.update(live2dModel, time);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            motionIndex = (motionIndex + 1 >= expressionMotions.Length ? 0 : motionIndex + 1);
            //expressionMotions[motionIndex].setLoop(true);
            motionQueueManager.startMotion(expressionMotions[motionIndex]);
        }
        motionQueueManager.updateParam(live2dModel);
        live2dModel.update();
    }
    private void OnRenderObject()
    {
        live2dModel.draw();
    }
    private void StartMotion(int index, int priority)
    {
        if (l2dMotionManager.getCurrentPriority() >= priority) return;
        l2dMotionManager.startMotion(motions[index]); 
    }
}
