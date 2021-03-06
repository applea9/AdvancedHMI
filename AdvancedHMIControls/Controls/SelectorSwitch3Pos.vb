﻿'**********************************************************************************************
'* Selector Switch for AdvancedHMI
'*
'* Archie Jacobs
'* Manufacturing Automation, LLC
'* support@advancedhmi.com
'* 
'*
'* Copyright 2011 Archie Jacobs
'*
'*
'* Distributed under the GNU General Public License (www.gnu.org)
'*
'* This program is free software; you can redistribute it and/or
'* as published by the Free Software Foundation; either version 2
'* of the License, or (at your option) any later version.
'*
'* This program is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'* GNU General Public License for more details.

'* You should have received a copy of the GNU General Public License
'* along with this program; if not, write to the Free Software
'* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
'*
'**********************************************************************************************

Public Class SelectorSwitch3Pos
    Inherits MfgControl.AdvancedHMI.Controls.SelectorSwitch3Pos


#Region "PLC Related Properties"
    '*****************************************************
    '* Property - Component to communicate to PLC through
    '*****************************************************
    Private m_CommComponent As AdvancedHMIDrivers.IComComponent
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property CommComponent() As AdvancedHMIDrivers.IComComponent
        Get
            Return m_CommComponent
        End Get
        Set(ByVal value As AdvancedHMIDrivers.IComComponent)
            If m_CommComponent IsNot value Then
                If SubScriptions IsNot Nothing Then
                    SubScriptions.UnsubscribeAll()
                End If

                m_CommComponent = value

                SubscribeToCommDriver()
            End If
        End Set
    End Property


    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private m_PLCAddressText As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressText() As String
        Get
            Return m_PLCAddressText
        End Get
        Set(ByVal value As String)
            If m_PLCAddressText <> value Then
                m_PLCAddressText = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private InvertVisible As Boolean
    Private m_PLCAddressVisible As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressVisible() As String
        Get
            Return m_PLCAddressVisible
        End Get
        Set(ByVal value As String)
            If m_PLCAddressVisible <> value Then
                m_PLCAddressVisible = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private m_PLCAddressValueLeft As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressValueLeft() As String
        Get
            Return m_PLCAddressValueLeft
        End Get
        Set(ByVal value As String)
            If m_PLCAddressValueLeft <> value Then
                m_PLCAddressValueLeft = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private m_PLCAddressValueRight As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressValueRight() As String
        Get
            Return m_PLCAddressValueRight
        End Get
        Set(ByVal value As String)
            If m_PLCAddressValueRight <> value Then
                m_PLCAddressValueRight = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private m_PLCAddressClickLeft As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressClickLeft() As String
        Get
            Return m_PLCAddressClickLeft
        End Get
        Set(ByVal value As String)
            If m_PLCAddressClickLeft <> value Then
                m_PLCAddressClickLeft = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Link to
    '*****************************************
    Private m_PLCAddressClickRight As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressClickRight() As String
        Get
            Return m_PLCAddressClickRight
        End Get
        Set(ByVal value As String)
            If m_PLCAddressClickRight <> value Then
                m_PLCAddressClickRight = value

                '* When address is changed, re-subscribe to new address
                SubscribeToCommDriver()
            End If
        End Set
    End Property
#End Region

#Region "Events"
    Private WithEvents tmrError As New System.Windows.Forms.Timer

    Private Sub ClickedLeft() Handles MyBase.SwitchLeft
        Try
            If m_PLCAddressClickLeft <> "" Then
                m_CommComponent.Write(m_PLCAddressClickLeft, 1)
            End If

            If m_PLCAddressClickRight <> "" Then
                m_CommComponent.Write(m_PLCAddressClickRight, 0)
            End If
        Catch ex As Exception
            DisplayError("WRITE FAILED!" & ex.Message)
        End Try
    End Sub

    Private Sub ClickedCenter() Handles MyBase.SwitchCenter
        Try
            If m_PLCAddressClickLeft <> "" Then
                m_CommComponent.Write(m_PLCAddressClickLeft, 0)
            End If

            If m_PLCAddressClickRight <> "" Then
                m_CommComponent.Write(m_PLCAddressClickRight, 0)
            End If
        Catch ex As Exception
            DisplayError("WRITE FAILED!" & ex.Message)
        End Try
    End Sub

    Private Sub ClickedRight() Handles MyBase.SwitchRight
        Try
            If m_PLCAddressClickLeft <> "" Then
                m_CommComponent.Write(m_PLCAddressClickLeft, 0)
            End If

            If m_PLCAddressClickRight <> "" Then
                m_CommComponent.Write(m_PLCAddressClickRight, 1)
            End If
        Catch ex As Exception
            DisplayError("WRITE FAILED!" & ex.Message)
        End Try
    End Sub


    '********************************************************************
    '* When an instance is added to the form, set the comm component
    '* property. If a comm component does not exist, add one to the form
    '********************************************************************
    Protected Overrides Sub OnCreateControl()
        MyBase.OnCreateControl()

        If Me.DesignMode Then
            '********************************************************
            '* Search for AdvancedHMIDrivers.IComComponent component in parent form
            '* If one exists, set the client of this component to it
            '********************************************************
            Dim i = 0
            Dim j As Integer = Me.Parent.Site.Container.Components.Count
            While m_CommComponent Is Nothing And i < j
                If Me.Parent.Site.Container.Components(i).GetType.GetInterface("AdvancedHMIDrivers.IComComponent") IsNot Nothing Then m_CommComponent = CType(Me.Parent.Site.Container.Components(i), AdvancedHMIDrivers.IComComponent)
                i += 1
            End While

            '************************************************
            '* If no comm component was found, then add one and
            '* point the CommComponent property to it
            '*********************************************
            If m_CommComponent Is Nothing And (PLCAddressClickLeft <> "" Or PLCAddressClickRight <> "") Then
                Me.Parent.Site.Container.Add(New AdvancedHMIDrivers.EthernetIPforPLCSLCMicroCom)
                m_CommComponent = CType(Me.Parent.Site.Container.Components(Me.Parent.Site.Container.Components.Count - 1), AdvancedHMIDrivers.IComComponent)
            End If
        Else
            SubscribeToCommDriver()
        End If
    End Sub



#End Region

#Region "Constructor/Destructor"
    '****************************************************************
    '* UserControl overrides dispose to clean up the component list.
    '****************************************************************
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                If SubScriptions IsNot Nothing Then
                    SubScriptions.dispose()
                End If
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
#End Region

#Region "Subscribing and PLC data receiving"
    Private SubScriptions As SubscriptionHandler
    '**************************************************
    '* Subscribe to addresses in the Comm(PLC) Driver
    '**************************************************
    Private Sub SubscribeToCommDriver()
        If Not DesignMode And IsHandleCreated Then
            '* Create a subscription handler object
            If SubScriptions Is Nothing Then
                SubScriptions = New SubscriptionHandler
                SubScriptions.CommComponent = m_CommComponent
                AddHandler SubScriptions.DisplayError, AddressOf DisplaySubscribeError
            End If

            '* Check through the properties looking for PLCAddress***, then see if the suffix matches an existing property
            Dim p() As Reflection.PropertyInfo = Me.GetType().GetProperties

            For i As Integer = 0 To p.Length - 1
                '* Does this property start with "PLCAddress"?
                If p(i).Name.IndexOf("PLCAddress", StringComparison.CurrentCultureIgnoreCase) = 0 Then
                    '* Get the property value
                    Dim PLCAddress As String = p(i).GetValue(Me, Nothing)
                    If PLCAddress <> "" Then
                        '* Get the text in the name after PLCAddress
                        Dim PropertyToWrite As String = p(i).Name.Substring(10)
                        Dim j As Integer = 0
                        '* See if there is a corresponding property with the extracted name
                        While j < p.Length AndAlso p(j).Name <> PropertyToWrite
                            j += 1
                        End While

                        '* If the proprty was found, then subscribe to the PLC Address
                        If j < p.Length Then
                            SubScriptions.SubscribeTo(PLCAddress, AddressOf PolledDataReturned, PropertyToWrite)
                        End If
                    End If
                End If
            Next
        End If
    End Sub

    '***************************************
    '* Call backs for returned data
    '***************************************
    Private OriginalText As String
    Private Sub PolledDataReturned(ByVal sender As Object, ByVal e As SubscriptionHandlerEventArgs)
        If e.PLCComEventArgs.ErrorId = 0 Then
            Try
                If e.PLCComEventArgs.Values IsNot Nothing AndAlso e.PLCComEventArgs.Values.Count > 0 Then
                    '* 13-NOV-14 Changed from Convert.ChangeType to CTypeDynamic because a 0/1 would not convert to boolean
                    '* Write the value to the property that came from the end of the PLCAddress... property name
                    Me.GetType().GetProperty(e.SubscriptionDetail.PropertyNameToSet). _
                                SetValue(Me, CTypeDynamic(e.PLCComEventArgs.Values(0), _
                                Me.GetType().GetProperty(e.SubscriptionDetail.PropertyNameToSet).PropertyType), Nothing)
                End If
            Catch ex As Exception
                DisplayError("INVALID VALUE!" & ex.Message)
            End Try
        Else
            DisplayError("Com Error " & e.PLCComEventArgs.ErrorId & "." & e.PLCComEventArgs.ErrorMessage)
        End If
    End Sub

    Private Sub DisplaySubscribeError(ByVal sender As Object, ByVal e As MfgControl.AdvancedHMI.Drivers.Common.PlcComEventArgs)
        DisplayError(e.ErrorMessage)
    End Sub
#End Region

#Region "Error Display"
    '********************************************************
    '* Show an error via the text property for a short time
    '********************************************************
    Private WithEvents ErrorDisplayTime As System.Windows.Forms.Timer
    Private Sub DisplayError(ByVal ErrorMessage As String)
        If ErrorDisplayTime Is Nothing Then
            ErrorDisplayTime = New System.Windows.Forms.Timer
            AddHandler ErrorDisplayTime.Tick, AddressOf ErrorDisplay_Tick
            ErrorDisplayTime.Interval = 5000
        End If

        '* Save the text to return to
        If Not ErrorDisplayTime.Enabled Then
            OriginalText = Me.Text
        End If

        ErrorDisplayTime.Enabled = True

        Me.Text = ErrorMessage
    End Sub


    '**************************************************************************************
    '* Return the text back to its original after displaying the error for a few seconds.
    '**************************************************************************************
    Private Sub ErrorDisplay_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ErrorDisplayTime.Tick
        Text = OriginalText

        If ErrorDisplayTime IsNot Nothing Then
            ErrorDisplayTime.Enabled = False
            ErrorDisplayTime.Dispose()
            ErrorDisplayTime = Nothing
        End If
    End Sub
#End Region

End Class
