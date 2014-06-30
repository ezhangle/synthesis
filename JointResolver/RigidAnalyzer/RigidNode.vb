﻿Imports Inventor

Public Class RigidNode
    Private level As Integer
    Public parent As RigidNode
    Public parentConnection As CustomRigidJoint
    Public children As New Dictionary(Of CustomRigidJoint, RigidNode)
    Public group As CustomRigidGroup

    Private Shared Function createRigidNode(ByRef jointDictionary As Dictionary(Of String, List(Of CustomRigidJoint)), ByRef nodeDictionary As Dictionary(Of String, RigidNode), ByRef groupz As CustomRigidGroup, Optional ByRef parentz As RigidNode = Nothing) As RigidNode
        Dim node As RigidNode = Nothing
        If (nodeDictionary.TryGetValue(groupz.fullQualifier, node)) Then
            Return node
        Else
            node = New RigidNode()
        End If
        node.parent = parentz
        node.parentConnection = Nothing
        If IsNothing(node.parent) Then
            node.level = 0
        Else
            node.level = node.parent.level + 1
        End If
        Console.WriteLine("Creating node for " & groupz.ToString() & " at level " & Str(node.level))
        node.group = groupz
        nodeDictionary(node.group.fullQualifier) = node

        Dim joints As List(Of CustomRigidJoint) = Nothing
        If (jointDictionary.TryGetValue(node.group.fullQualifier, joints)) Then
            For Each joint As CustomRigidJoint In joints
                Dim childGroup As CustomRigidGroup = If(joint.groupOne.Equals(node.group), joint.groupTwo, joint.groupOne)
                If nodeDictionary.ContainsKey(childGroup.fullQualifier) Then Continue For
                node.children(joint) = createRigidNode(jointDictionary, nodeDictionary, childGroup, node)
                node.children(joint).parentConnection = joint
            Next joint
        End If
        Return node
    End Function

    Public Shared Function generateNodeTree(ByRef results As CustomRigidResults) As RigidNode
        Dim jointDictionary As New Dictionary(Of String, List(Of CustomRigidJoint))
        Dim baseGroup As CustomRigidGroup = Nothing
        For Each group As CustomRigidGroup In results.groups
            jointDictionary(group.fullQualifier) = New List(Of CustomRigidJoint)
            If (group.grounded) Then baseGroup = group
        Next group
        If IsNothing(baseGroup) Then Return Nothing
        For Each joint As CustomRigidJoint In results.joints
            jointDictionary(joint.groupOne.fullQualifier).Add(joint)
            jointDictionary(joint.groupTwo.fullQualifier).Add(joint)
        Next joint

        Dim nodeDictionary As New Dictionary(Of String, RigidNode)
        Return createRigidNode(jointDictionary, nodeDictionary, baseGroup)
    End Function

    Public Overrides Function ToString() As String
        Dim result As String = Space(3 * level) & "Rigid Node" & vbNewLine & _
        Space(3 * level) & "Name: " & group.ToString() & vbNewLine
        If children.Count > 0 Then
            result &= Space(3 * level) & "Children: "
            For Each pair As KeyValuePair(Of CustomRigidJoint, RigidNode) In children
                result &= vbNewLine & Space(3 * level + 1) & "- " & pair.Key.ToString()
                result &= vbNewLine & pair.Value.ToString()
            Next pair
        End If
        Return result
    End Function

    Public Sub listAllNodes(ByRef list As List(Of RigidNode))
        list.Add(Me)
        For Each pair As KeyValuePair(Of CustomRigidJoint, RigidNode) In children
            pair.Value.listAllNodes(list)
        Next pair
    End Sub
End Class
