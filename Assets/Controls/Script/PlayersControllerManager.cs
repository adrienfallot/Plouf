using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersControllerManager : MonoBehaviour
{
    public Player player1 = null;
    public Player player2 = null;
    public Player player3 = null;
    public Player player4 = null;

    private List<PlayerControllerAxis> m_ControllersAxis = new List<PlayerControllerAxis>();

    private string m_HorizontalP1Axis = "Horizontal_P1";
    private string m_VerticalP1Axis = "Vertical_P1";
    private string m_DashP1Axis = "Dash_P1";
    private string m_JumpP1Axis = "Jump_P1";
    private string m_FireP1Axis = "Fire_P1";

    private string m_HorizontalP2Axis = "Horizontal_P2";
    private string m_VerticalP2Axis = "Vertical_P2";
    private string m_DashP2Axis = "Dash_P2";
    private string m_JumpP2Axis = "Jump_P2";
    private string m_FireP2Axis = "Fire_P2";

    private string m_HorizontalP3Axis = "Horizontal_P3";
    private string m_VerticalP3Axis = "Vertical_P3";
    private string m_DashP3Axis = "Dash_P3";
    private string m_JumpP3Axis = "Jump_P3";
    private string m_FireP3Axis = "Fire_P3";

    private string m_HorizontalP4Axis = "Horizontal_P4";
    private string m_VerticalP4Axis = "Vertical_P4";
    private string m_DashP4Axis = "Dash_P4";
    private string m_JumpP4Axis = "Jump_P4";
    private string m_FireP4Axis = "Fire_P4";

    void Start ()
    {
        PlayerControllerAxis player1HorizontalAxis = new ContinuousPlayerControllerAxis(player1, m_HorizontalP1Axis, player1.MoveHorizontal);
        PlayerControllerAxis player1VerticalAxis = new ContinuousPlayerControllerAxis(player1, m_VerticalP1Axis, player1.MoveVertical);
        PlayerControllerAxis player1DashAxis = new ContinuousPlayerControllerAxis(player1, m_DashP1Axis, player1.Dash);
        PlayerControllerAxis player1JumpAxis = new SpontaneousPlayerControllerAxis(player1, m_JumpP1Axis, player1.Jump);
        PlayerControllerAxis player1AimAxis = new SpontaneousPlayerControllerAxis(player1, m_FireP1Axis, player1.Aim);
        PlayerControllerAxis player1FireAxis = new SpontaneousReleasePlayerControllerAxis(player1, m_FireP1Axis, player1.Fire);

        PlayerControllerAxis player2HorizontalAxis = new ContinuousPlayerControllerAxis(player2, m_HorizontalP2Axis, player2.MoveHorizontal);
        PlayerControllerAxis player2VerticalAxis = new ContinuousPlayerControllerAxis(player2, m_VerticalP2Axis, player2.MoveVertical);
        PlayerControllerAxis player2DashAxis = new ContinuousPlayerControllerAxis(player2, m_DashP2Axis, player2.Dash);
        PlayerControllerAxis player2JumpAxis = new SpontaneousPlayerControllerAxis(player2, m_JumpP2Axis, player2.Jump);
        PlayerControllerAxis player2AimAxis = new SpontaneousPlayerControllerAxis(player2, m_FireP2Axis, player2.Aim);
        PlayerControllerAxis player2FireAxis = new SpontaneousReleasePlayerControllerAxis(player2, m_FireP2Axis, player2.Fire);

        PlayerControllerAxis player3HorizontalAxis = new ContinuousPlayerControllerAxis(player3, m_HorizontalP3Axis, player3.MoveHorizontal);
        PlayerControllerAxis player3VerticalAxis = new ContinuousPlayerControllerAxis(player3, m_VerticalP3Axis, player3.MoveVertical);
        PlayerControllerAxis player3DashAxis = new ContinuousPlayerControllerAxis(player3, m_DashP3Axis, player3.Dash);
        PlayerControllerAxis player3JumpAxis = new SpontaneousPlayerControllerAxis(player3, m_JumpP3Axis, player3.Jump);
        PlayerControllerAxis player3AimAxis = new SpontaneousPlayerControllerAxis(player3, m_FireP3Axis, player3.Aim);
        PlayerControllerAxis player3FireAxis = new SpontaneousReleasePlayerControllerAxis(player3, m_FireP3Axis, player3.Fire);

        PlayerControllerAxis player4HorizontalAxis = new ContinuousPlayerControllerAxis(player4, m_HorizontalP4Axis, player4.MoveHorizontal);
        PlayerControllerAxis player4VerticalAxis = new ContinuousPlayerControllerAxis(player4, m_VerticalP4Axis, player4.MoveVertical);
        PlayerControllerAxis player4DashAxis = new ContinuousPlayerControllerAxis(player4, m_DashP4Axis, player4.Dash);
        PlayerControllerAxis player4JumpAxis = new SpontaneousPlayerControllerAxis(player4, m_JumpP4Axis, player4.Jump);
        PlayerControllerAxis player4AimAxis = new SpontaneousPlayerControllerAxis(player4, m_FireP4Axis, player4.Aim);
        PlayerControllerAxis player4FireAxis = new SpontaneousReleasePlayerControllerAxis(player4, m_FireP4Axis, player4.Fire);


        m_ControllersAxis.Add(player1HorizontalAxis);
        m_ControllersAxis.Add(player1VerticalAxis);
        m_ControllersAxis.Add(player1DashAxis);
        m_ControllersAxis.Add(player1JumpAxis);
        m_ControllersAxis.Add(player1AimAxis);
        m_ControllersAxis.Add(player1FireAxis);
        m_ControllersAxis.Add(player2HorizontalAxis);
        m_ControllersAxis.Add(player2VerticalAxis);
        m_ControllersAxis.Add(player2DashAxis);
        m_ControllersAxis.Add(player2JumpAxis);
        m_ControllersAxis.Add(player2AimAxis);
        m_ControllersAxis.Add(player2FireAxis);
        m_ControllersAxis.Add(player3HorizontalAxis);
        m_ControllersAxis.Add(player3VerticalAxis);
        m_ControllersAxis.Add(player3DashAxis);
        m_ControllersAxis.Add(player3JumpAxis);
        m_ControllersAxis.Add(player3AimAxis);
        m_ControllersAxis.Add(player3FireAxis);
        m_ControllersAxis.Add(player4HorizontalAxis);
        m_ControllersAxis.Add(player4VerticalAxis);
        m_ControllersAxis.Add(player4DashAxis);
        m_ControllersAxis.Add(player4JumpAxis);
        m_ControllersAxis.Add(player4AimAxis);
        m_ControllersAxis.Add(player4FireAxis);
    }
	
	void Update ()
    {
        foreach(PlayerControllerAxis controllerAxis in m_ControllersAxis)
        {
            controllerAxis.ExecuteInput();
        }        
	}
}
