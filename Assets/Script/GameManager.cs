﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;

public class GameManager : MonoBehaviour {

	public CharacterController character;
	public CapitaineScript capitaine;
	public PantaloneScript pantalone;
	public ColombineScript colombine;
	public SouffleurScript souffleur;
	public PierrotScript pierrot;
	public CoffreScript coffre;
	public PublicScript publicOnScene;
	public BoxCollider2D cloche;
	public CameraScript camera;
	public GUIManager guiManager;
	public FadeBlackScript fadeBlack;

    public TextAsset GameAsset;

    private List<Character> characterList;
    private List<Evenement> eventList;
    private CoroutineParameter param;


	bool test = false;

	// Use this for initialization
	void Start () {
        characterList = new List<Character>();

        characterList.Add(new Character(character.gameObject, "Arlequin"));
        characterList.Add(new Character(capitaine.gameObject, "Capitaine"));
        characterList.Add(new Character(pantalone.gameObject, "Pantalone"));
        characterList.Add(new Character(colombine.gameObject, "Colombine"));
        characterList.Add(new Character(pierrot.gameObject, "Pierrot"));

        param = new CoroutineParameter();

        eventList = loadEvent(GameAsset);
		startEvent("Tutorial_3", eventList);
    }

	// Update is called once per frame
	void Update () {

		if(test)
			StartCoroutine (event1 ());
		test = false;

	}

	public void launchEndEvent(){
		StartCoroutine (eventFinTuto());
	}

    //Lance un evenement dans le jeu
    public void startEvent(string id, List<Evenement> eventList)
    {
        for (int i = 0; i < eventList.Count; i++ )
        {
            if (id == eventList[i]._id)
            {
                StartCoroutine(launchEvent(eventList[i]._event));  
            }
        }
    }

    IEnumerator launchEvent(List<IEnumerator> list)
    {
        param._count += 1;
        int i = 0;

        while (i < list.Count)
        {
            if (param._count > 0)
            {
                param._count = 0;
                Debug.Log("Execution de l'action n°" + i + ".");
                StartCoroutine(list[i]);
                i++;
            }
            yield return null;
        }
        Debug.Log("Fin de l'event");

        yield break;
    }

    //permet de recuperer le gameobject d'un personnage a partir de son nom
    private GameObject getCharacterGameobject(string name){
        
        for (int i = 0; i < characterList.Count; i++){
           // Debug.Log("research : " + name + " , actually this is :" + characterList[i]._characterName);
            if (name == characterList[i]._characterName){
               // Debug.Log("Found");
                return characterList[i]._characterGameobject;
            }
        }
        //Debug.Log("Not Found");
        return null;
    }

    //Chargment d'un event a partir d'un fichier xml
    public List<Evenement> loadEvent(TextAsset GameAsset)
    {
        XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
        xmlDoc.LoadXml(GameAsset.text); // load the file.

        XmlNodeList eventList = xmlDoc.GetElementsByTagName("event"); // liste des evenements en xml
        List<Evenement> evenementList = new List<Evenement>(); // liste des evenements
        
         for (int temp = 0; temp < eventList.Count; temp++)
         {
             XmlNodeList actionsList = eventList[temp].ChildNodes;
             List<IEnumerator> coroutineList = new List<IEnumerator>();
             Evenement event_1 = new Evenement(coroutineList, eventList[temp].Attributes["id"].Value);
             evenementList.Add(event_1);

             Debug.Log(actionsList.Count + " actions a charger.");
             for (int i = 0; i < actionsList.Count; i++)
             {
                 //Debug.Log(" nom de la node n°" + i + " :" + actionsList.Item(i).Name);
                 //analyse de la node
                 if (checkNode(actionsList[i], coroutineList, param)) { }
                 else if (actionsList.Item(i).Name == "multiple")
                 {
                     //Debug.Log("Node multiple detecté ");
                     CoroutineParameter paramMultiple = new CoroutineParameter();
                     List<IEnumerator> multipleCoroutineList = new List<IEnumerator>();
                     IEnumerator action = multipleCoroutine(multipleCoroutineList, paramMultiple, param);
                     coroutineList.Insert(coroutineList.Count, action);

                     //remplir la liste des coroutines multiples
                     XmlNodeList multipleActionsList = actionsList.Item(i).ChildNodes;
                     for (int j = 0; j < multipleActionsList.Count; j++)
                     {
                         checkNode(multipleActionsList[j], multipleCoroutineList, paramMultiple);
                     }
                 }
             }

         }
        return evenementList;
    }

    bool checkWaitAttribute(XmlNode node)
    {
        bool wait = true;
        if (node != null)
        {
            if (node.Value == "false" || node.Value == "False")
            {
                wait = false;
            }
        }
        return wait;
    }

    bool checkNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (checkCharacterNode(node, coroutineList, param)) { return true; }
        //activation/desactivation du GUImanager
        else if (checkGUIManagerNode(node, coroutineList, param)) { return true; }
        //action du souffleur
        else if (checkSouffleurNode(node, coroutineList, param)) { return true; }
        //action du public
        else if (checkPublicNode(node, coroutineList, param)) { return true; }
        //action de la camera
        else if (checkCameraNode(node, coroutineList, param)) { return true; }
        //wait coroutine
        else if (checkWaitNode(node, coroutineList, param)) { return true; }
        //fadetoblack coroutine
        else if (checkFadeNode(node, coroutineList, param)) { return true; }
        else
            return false;
    }

    bool checkWaitNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "wait")
        {
            //Debug.Log("Attente de " +node.Attributes["time"].Value +"s.");
            IEnumerator action = waitCoroutine(float.Parse(node.Attributes["time"].Value), param);
            coroutineList.Insert(coroutineList.Count, action);
            return true;
        }
        return false;
    }

    bool checkFadeNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "fadetoblack")
        {
            //Debug.Log("Fade de " + node.Attributes["time"].Value + "s.");
            IEnumerator action = fadeCoroutine(float.Parse(node.Attributes["time"].Value), param);
            coroutineList.Insert(coroutineList.Count, action);
            return true;
        }
        return false;
    }

    bool checkPublicNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "public")
        {
           // Debug.Log("On cible le public");
            XmlNodeList publicActionsList = node.ChildNodes;
            for (int j = 0; j < publicActionsList.Count; j++)
            {
                if (publicActionsList.Item(j).Name == "laugh")
                {
                    //Debug.Log("Le public rigole pendant " + publicActionsList.Item(j).Attributes["time"].Value + "s.");
                    IEnumerator action = laughCoroutine(float.Parse(publicActionsList.Item(j).Attributes["time"].Value), param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
                if (publicActionsList.Item(j).Name == "addvalue")
                {
                    IEnumerator action = publicValueCoroutine(float.Parse(publicActionsList.Item(j).Attributes["value"].Value), 0, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
                if (publicActionsList.Item(j).Name == "subvalue")
                {
                    IEnumerator action = publicValueCoroutine(float.Parse(publicActionsList.Item(j).Attributes["value"].Value), 1, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
                if (publicActionsList.Item(j).Name == "setvalue")
                {
                    IEnumerator action = publicValueCoroutine(float.Parse(publicActionsList.Item(j).Attributes["value"].Value), 2, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
            }
            return true;
        }
        return false;
    }

    bool checkCameraNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "camera")
        {
            //Debug.Log("On cible la camera");

            XmlNodeList cameraActionsList = node.ChildNodes;
            for (int j = 0; j < cameraActionsList.Count; j++)
            {
                if (cameraActionsList.Item(j).Name == "deplacement")
                {
                    /*Debug.Log("Deplacement de la camera en : " + cameraActionsList[j].Attributes["x"].Value + ", " +
                              cameraActionsList[j].Attributes["y"].Value + ", " +
                              cameraActionsList[j].Attributes["z"].Value + " .");*/

              
                    bool wait = checkWaitAttribute( cameraActionsList[j].Attributes["wait"]);

                    IEnumerator action = deplacementCameraCoroutine(new Vector3(
                     float.Parse(cameraActionsList[j].Attributes["x"].Value),
                     float.Parse(cameraActionsList[j].Attributes["y"].Value),
                     float.Parse(cameraActionsList[j].Attributes["z"].Value)), wait, param);

                    coroutineList.Insert(coroutineList.Count, action);
                }
                if (cameraActionsList.Item(j).Name == "reset")
                {
                    //Debug.Log("Reset de la position de la camera.");
                    bool wait = checkWaitAttribute(cameraActionsList[j].Attributes["wait"]);
                    IEnumerator action = resetCameraCoroutine(wait, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
            }
            return true;
        }
        return false;
    }

    bool checkSouffleurNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "souffleur")
        {
           // Debug.Log("On cible le souffleur ");

            XmlNodeList characterActionsList = node.ChildNodes;
            for (int j = 0; j < characterActionsList.Count; j++)
            {
                if (characterActionsList.Item(j).Name == "talk")
                {
                   // Debug.Log("Le souffleur veut dire un truc ");
                    XmlNodeList souffleurText = characterActionsList.Item(j).ChildNodes;
                    List<string> text = new List<string>();
                    for (int k = 0; k < souffleurText.Count; k++)
                    {
                        text.Add(souffleurText[k].InnerText);
                    }
                    IEnumerator action = talkCoroutine(text, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
                else if (characterActionsList.Item(j).Name == "feedback")
                {
                    /*Debug.Log("Le souffleur envoie un feedback de type : " + characterActionsList[j].Attributes["type"].Value +
                              " d'une durée de " + characterActionsList[j].Attributes["time"].Value + "s.");*/
                    IEnumerator action = feedbackCoroutine(characterActionsList[j].Attributes["type"].Value, float.Parse(characterActionsList[j].Attributes["time"].Value), param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
            }
            return true;
        }
        return false;
    }

    bool checkGUIManagerNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "guimanager" || node.Name == "guiManager")
        {
            //Debug.Log("Modification du GUIManager en " + node.Attributes["active"].Value + ".");
            bool temp = false;
            if (node.Attributes["active"].Value == "true" ||
                node.Attributes["active"].Value == "True" ||
                node.Attributes["active"].Value == "Vrai" ||
                node.Attributes["active"].Value == "vrai")
            {
                temp = true;
            }
            else if (node.Attributes["active"].Value == "false" ||
                node.Attributes["active"].Value == "False" ||
                node.Attributes["active"].Value == "Faux" ||
                node.Attributes["active"].Value == "faux")
            {
                temp = false;
            }
            IEnumerator action = guiCoroutine(temp, param);
            coroutineList.Insert(coroutineList.Count, action);
            return true;
        }
        return false;
    }

    bool checkCharacterNode(XmlNode node, List<IEnumerator> coroutineList, CoroutineParameter param)
    {
        if (node.Name == "character")
        {
           // Debug.Log("On cible le personnage " +node.Attributes["name"].Value + ".");
            XmlNodeList characterActionsList = node.ChildNodes;
            for (int j = 0; j < characterActionsList.Count; j++)
            {
                if (characterActionsList.Item(j).Name == "deplacement")
                {
                    /*Debug.Log("Deplacement du personnage en : " + characterActionsList[j].Attributes["x"].Value + ", " +
                              characterActionsList[j].Attributes["y"].Value + ", " +
                              characterActionsList[j].Attributes["z"].Value + " .");*/

                    bool wait = checkWaitAttribute(characterActionsList[j].Attributes["wait"]);

                    XmlNode instantNode = characterActionsList[j].Attributes["instant"];
                    bool instant = false;
                    if (instantNode != null)
                    {
                        instant = false;
                        if (instantNode.Value == "true" || instantNode.Value == "True")
                        {
                            instant = true;
                        }
                    }

                    IEnumerator action = deplacementCoroutine(node.Attributes["name"].Value,
                     new Vector3(
                     float.Parse(characterActionsList[j].Attributes["x"].Value),
                     float.Parse(characterActionsList[j].Attributes["y"].Value),
                     float.Parse(characterActionsList[j].Attributes["z"].Value)), instant,  wait, param);

                    coroutineList.Insert(coroutineList.Count, action);
                }
                else if (characterActionsList.Item(j).Name == "animation")
                {
                    //Debug.Log("Activation de l'animation d'un personnage : " + characterActionsList[j].Attributes["name"].Value);
                    bool wait = checkWaitAttribute(characterActionsList[j].Attributes["wait"]);
                    IEnumerator action = animationCoroutine(node.Attributes["name"].Value, characterActionsList[j].Attributes["name"].Value, wait, param);
                    coroutineList.Insert(coroutineList.Count, action);
                }
                else if (characterActionsList.Item(j).Name == "rotation")
                {
                   /*Debug.Log("Rotation du personnage de : " + characterActionsList[j].Attributes["x"].Value + ", " +
                              characterActionsList[j].Attributes["y"].Value + ", " +
                          characterActionsList[j].Attributes["z"].Value + " .");*/
                    bool wait = checkWaitAttribute(characterActionsList[j].Attributes["wait"]);
                    IEnumerator action = rotationCoroutine(node.Attributes["name"].Value,
                    new Vector3(
                        float.Parse(characterActionsList[j].Attributes["x"].Value),
                        float.Parse(characterActionsList[j].Attributes["y"].Value),
                        float.Parse(characterActionsList[j].Attributes["z"].Value)) , param);

                    coroutineList.Insert(coroutineList.Count, action);
                }
            }
            return true;
        }
        return false;
    }

    IEnumerator guiCoroutine(bool active, CoroutineParameter param )
    {
        if (active)
            Debug.Log("Activation du GUImanager.");
        else 
            Debug.Log("Desactivation du GUImanager.");
        guiManager.active = active;
        param._count++;
        yield break;
    }

    IEnumerator deplacementCoroutine(string characterName, Vector3 position, bool instant, bool wait, CoroutineParameter param)
    {
        Debug.Log("Execution d'un deplacement de " + characterName + " en " + position + ".");
        CharacterController character = getCharacterGameobject(characterName).GetComponent<CharacterController>();
        if (instant)
        {
            character.setPositionAndGoal(position);
        }
        else
        {
            character.goTo(position);
        }

        if (wait)
        {
            while (character.transform.position != position)
            {
                yield return null;
            }
        }
        param._count++;
        yield break;
    }

    IEnumerator rotationCoroutine(string characterName, Vector3 rotation, CoroutineParameter param ) 
    {
        Debug.Log("Execution d'une rotation de " + characterName + " en " + rotation + ".");
        GameObject character = getCharacterGameobject(characterName);

        character.transform.Rotate(rotation);
        param._count++;
        yield break;
    }

    IEnumerator animationCoroutine(string characterName, string animationName, bool wait, CoroutineParameter param )
    {
        Debug.Log("Execution d'une animation de " + characterName + " , qui est \"" + animationName + "\".");
        Animator characterAnimator = getCharacterGameobject(characterName).GetComponentInChildren<Animator>();

        characterAnimator.SetTrigger (animationName);
        if (wait) {
		    while(characterAnimator.GetCurrentAnimatorStateInfo (0).shortNameHash !=  Animator.StringToHash(animationName) )
            {
                 Debug.Log("recherche du state");
				    yield return null;
		    }
            Debug.Log("trouvé");

		    yield return new WaitForSeconds(characterAnimator.GetCurrentAnimatorStateInfo(0).length);
         }
        param._count++;
        yield break;
    }

    IEnumerator feedbackCoroutine(string type, float time, CoroutineParameter param)
    {
        Debug.Log("Envoie d'un feedback de type :" + type + " , de " + time + "s.");
        if(type == "good")
        {
            souffleur.giveFeedback(time, 0);
        }
        else if(type == "bad")
        {
            souffleur.giveFeedback(time, 1);
        }
        param._count++;
        yield break;
    }

    IEnumerator talkCoroutine(List<string> text, CoroutineParameter param )
    {
        Debug.Log("Le souffleur parle.");
        souffleur.saySomething(text, false);
        while (souffleur.talking == true)
        {
            yield return null;
        }
        param._count++;
        yield break;
    }

    IEnumerator laughCoroutine(float time, CoroutineParameter param)
    {
        Debug.Log("Le public rigole.");
        publicOnScene.happy(time);
        param._count++;
        yield break;
    }

    IEnumerator deplacementCameraCoroutine(Vector3 position, bool wait,  CoroutineParameter param )
    {
        Debug.Log("La camera se deplace en :" + position + ".");
        camera.moveTo(position);
        if (wait)
        {
            while ((position - camera.transform.position).magnitude > 1)
            {
                yield return null;
            }
        }
        param._count++;
        yield break;
    }

    IEnumerator resetCameraCoroutine(bool wait, CoroutineParameter param )
    {
        Debug.Log("On reset la position de la camera");
        camera.resetPosition();
        if (wait)
        {
            while ((camera.getOriginalPosition() - camera.transform.position).magnitude > 1)
            {
                yield return null;
            }
        }
        param._count++;
        yield break;
    }

    IEnumerator waitCoroutine(float time, CoroutineParameter param ) 
    {
        yield return new WaitForSeconds(time);
        param._count++;
        yield break;
    }

    IEnumerator fadeCoroutine(float time, CoroutineParameter param )
    {
        Debug.Log("Fadetoblack de " + time + "s.");
        fadeBlack.fadeToBlack(time);
        yield return new WaitForSeconds( time + 2 / fadeBlack.fadeSpeed);
        param._count++;
        yield break;
    }

    IEnumerator publicValueCoroutine(float value, int type, CoroutineParameter param)
    {
        if (type == 0)
        {
            publicOnScene.addValue(value);
        }
        else if (type == 1)
        {
            publicOnScene.subValue(value);
        }
        else if (type == 2)
        {
            publicOnScene.setValue(value);
        }
        param._count++;
        yield break;
    }

    IEnumerator multipleCoroutine(List<IEnumerator> list, CoroutineParameter multipleParam, CoroutineParameter param)
    {
        //On demarre toute les coroutines
        for (int i = 0; i < list.Count; i++)
        {
            StartCoroutine(list[i]);
        }
        //Ojn attend la fin de chaque coroutine
        while (multipleParam._count != list.Count)
        {
            yield return null;
        }
        param._count++;
        yield break;
    }

    

	//Intro avec le souffleur
	public IEnumerator event1(){

		guiManager.active = false;

		Vector3 moveEvent1 =  new Vector3(-16,7,30);
		character.goTo (moveEvent1);

		while (character.transform.position != moveEvent1) {
			yield return null;
		}

		souffleur.saySomething (souffleur.textList1, false);

		while (souffleur.talking == true) {
			yield return null;
		}

		Vector3 zoomPublicEvent = new Vector3(0,6,0);

		publicOnScene.addValue (20);
		souffleur.saySomething (souffleur.textList2, false);
		camera.moveTo (zoomPublicEvent);

		while (souffleur.talking == true) {
			yield return null;
		}
		camera.resetPosition ();

		guiManager.active = false;
		yield return new WaitForSeconds (1.0f);

		Vector3 zoomCoffreEvent = new Vector3(17,10,16);

		souffleur.saySomething (souffleur.textList3, false);
		camera.moveTo (zoomCoffreEvent);

		while (souffleur.talking == true) {
			yield return null;
		}
		camera.resetPosition ();

		guiManager.active = true;

		yield break;

	}

	//Arrivé du capitaine
	public IEnumerator event2(){

		guiManager.active = false;
		
		Vector3 moveEvent1 =  new Vector3(-16,7,30);
		capitaine.GetComponent<CharacterController>().goTo (moveEvent1);

		while (capitaine.transform.position !=  moveEvent1) {
			yield return null;
		}

		capitaine.GetComponent<CharacterController>().sprite.SetTrigger ("moquerie");

		capitaine.GetComponent<AudioSource> ().PlayOneShot (capitaine.moquerie,1);

		souffleur.saySomething (souffleur.textList5, false);
		while (souffleur.talking == true) {
			yield return null;
		}

		//activation du capitaine
		capitaine.GetComponent<Collider2D> ().enabled = true;

		guiManager.active = true;

		yield break;
		
	}

    //Arrivé de pantalone et colombine
    public IEnumerator event3(){
        
        guiManager.active = false;

		//fondu noir 
		fadeBlack.fadeToBlack (1.0f);
		yield return new WaitForSeconds (1/fadeBlack.fadeSpeed);
		yield return new WaitForSeconds (1);

		coffre.gameObject.SetActive (false);
		pantalone.transform.position = new Vector3 (-16, 7, 30);
		character.setPositionAndGoal (new Vector3 (-8, 7, 30));
		colombine.transform.position = new Vector3 (16, 7, 30);

		yield return new WaitForSeconds (1/fadeBlack.fadeSpeed);
		yield return new WaitForSeconds (0.5f);

		Vector3 zoomPantaloneEvent = new Vector3(-15,12,11);
		Vector3 zoomColombineEvent = new Vector3(17,10,16);
		bool eventPantaloneDone = false, eventColombineDone = false;

        souffleur.saySomething (souffleur.textList6, false);

        while (souffleur.talking == true) {

			if(!eventPantaloneDone && souffleur.getIndex() == 1){
				camera.moveTo (zoomPantaloneEvent);
			}
			if(!eventColombineDone && souffleur.getIndex() == 2){
				camera.moveTo (zoomColombineEvent);				
            }
            yield return null;
		}

        camera.resetPosition ();
		yield return new WaitForSeconds (2.0f);

		pantalone.GetComponentInChildren<Animator> ().SetTrigger("asking");
		pantalone.GetComponentInChildren<bulleInfoScript> ().showBubble (1.5f);
		yield return new WaitForSeconds(pantalone.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length);

        guiManager.active = true;
        
        yield break;
        
    }

	//Arrivé du lazzi
	public IEnumerator lazziEvent(){

		guiManager.active = false;

		publicOnScene.subValue (100);

		souffleur.saySomething (souffleur.textList7, false);

		Vector3 zoomClocheEvent = new Vector3 (20, 23, 27);
		
		while (souffleur.talking == true) {
			if(souffleur.getIndex() == 1){
				camera.moveTo(zoomClocheEvent);
				cloche.enabled = true;
				break;
			}
			yield return null;
		}
		while (souffleur.talking == true) {
			yield return null;			
		}
		camera.resetPosition();
		
		guiManager.active = true;

		yield break;
	}

    //Event avec pierrot
	IEnumerator eventFinTuto(){
		
		guiManager.active = false;

		Vector3 goToCenterEvent = new Vector3 (-1.5f, 10, 30);
		Vector3 zoomLazziEvent = new Vector3 (0, 13, 5);
		
		pierrot.GetComponent<CharacterController>().goTo (goToCenterEvent);
		while (pierrot.transform.position !=  goToCenterEvent) {
			yield return null;
		}
		
		//zoom
		camera.moveTo (zoomLazziEvent);
		
		pierrot.GetComponentInChildren<Animator> ().SetTrigger ("juggling");
		while(pierrot.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo (0).shortNameHash !=  Animator.StringToHash("pierrot_jongle") ){
			yield return null;
		}
		yield return new WaitForSeconds(pierrot.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length );
		
		publicOnScene.setValue (75);
				
		Vector3 goTalkToTpantalone = new Vector3 (-8, 7, 30);
		character.goTo (goTalkToTpantalone);
		while (character.transform.position !=  goTalkToTpantalone) {
			yield return null;
		}	

		pantalone.GetComponentInChildren<Animator> ().SetTrigger ("dispute");
		character.GetComponentInChildren<Animator> ().SetTrigger ("angryTalking");
		yield return new WaitForSeconds(pantalone.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length);

		Vector3 moveToColombine = new Vector3 (11, 10, 30);
		pierrot.GetComponent<CharacterController>().goTo (moveToColombine);
		while (pierrot.transform.position !=  moveToColombine) {
			yield return null;
		}
		pierrot.GetComponentInChildren<Animator> ().SetTrigger ("range");
		colombine.gameObject.SetActive (false);

		while(pierrot.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo (0).shortNameHash != Animator.StringToHash("pierrot_range_colombine") ){
			yield return null;
		}
		yield return new WaitForSeconds(pierrot.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length);
		

		Vector3 goAwayEvent = new Vector3 (50f, 10, 30);
		pierrot.GetComponent<CharacterController>().goTo (goAwayEvent);
		while (pierrot.transform.position !=  goAwayEvent) {
			yield return null;
		}

		// compte rendu 
		//fadeBlack.fadeToBlack (Mathf.Infinity);
		GameObject.Find ("Gazette").GetComponent<Image>().color = new Color (1, 1, 1, 1);

		while (true) {
			if(Input.GetButtonDown("Fire1")){
				Application.LoadLevel("saynete_1");
				break;
			}
			yield return null;
		}

		yield break;
	}

}

class Character
{
    public GameObject _characterGameobject;
    public string _characterName;
    public Character(GameObject g, string s )
    {
        _characterGameobject = g;
        _characterName = s;
    }
}
public class Evenement
{
    public List<IEnumerator> _event;
    public string _id;
    public Evenement(List<IEnumerator> i, string s)
    {
        _event = i;
        _id = s;
    }
}

class CoroutineParameter
{
    public int _count;
    public CoroutineParameter()
    {
        _count = 0;
    }
}