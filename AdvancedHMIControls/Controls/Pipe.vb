﻿'****************************************************************************
'* Archie Jacobs
'* Manufacturing Automation, LLC
'* support@advancedhmi.com
'* 12-JUN-11
'*
'* Copyright 2011 Archie Jacobs
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
'* 12-JUN-11 Created
'****************************************************************************
Public Class Pipe
    Inherits MfgControl.AdvancedHMI.Controls.Pipe


#Region "Basic Properties"
    Private SavedBackColor As System.Drawing.Color

    '******************************************************************************************
    '* Use the base control's text property and make it visible as a property on the designer
    '******************************************************************************************
    <System.ComponentModel.Browsable(True)> _
<System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)> _
Public Overrides Property Text() As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            If m_Format <> "" And (Not DesignMode) Then
                Try
                    MyBase.Text = _Prefix & Format(CSng(value) * _ValueScaleFactor, m_Format) & _Suffix
                Catch ex As Exception
                    MyBase.Text = "Check NumericFormat and variable type"
                End Try
            Else
                If _ValueScaleFactor = 1 Then
                    MyBase.Text = _Prefix & value & _Suffix
                Else
                    Try
                        MyBase.Text = value * _ValueScaleFactor
                    Catch ex As Exception
                        DisplayError("Scale Factor Error - " & ex.Message)
                    End Try
                End If

                '* Highlight in red if an exclamation mark is in text
                If InStr(value, _HighlightKeyChar) > 0 Then
                    If MyBase.ForeColor <> _Highlightcolor Then SavedBackColor = MyBase.ForeColor
                    MyBase.ForeColor = _Highlightcolor
                Else
                    If SavedBackColor <> Nothing Then MyBase.ForeColor = SavedBackColor
                End If
            End If

            'If _Prefix <> "" Or _Suffix <> "" Then
            '    MyBase.Text = _Prefix & MyBase.Text & _Suffix
            'End If
        End Set
    End Property

    '**********************************
    '* Prefix and suffixes to text
    '**********************************
    Private _Prefix As String
    Public Property TextPrefix() As String
        Get
            Return _Prefix
        End Get
        Set(ByVal value As String)
            _Prefix = value
            Invalidate()
        End Set
    End Property

    Private _Suffix As String
    Public Property TextSuffix() As String
        Get
            Return _Suffix
        End Get
        Set(ByVal value As String)
            _Suffix = value
            Invalidate()
        End Set
    End Property


    '***************************************************************
    '* Property - Highlight Color
    '***************************************************************
    Private _Highlightcolor As Drawing.Color = Drawing.Color.Red
    <System.ComponentModel.Category("Appearance")> _
    Public Property HighlightColor() As Drawing.Color
        Get
            Return _Highlightcolor
        End Get
        Set(ByVal value As Drawing.Color)
            _Highlightcolor = value
        End Set
    End Property

    Private _HighlightKeyChar As String = "!"
    <System.ComponentModel.Category("Appearance")> _
    Public Property HighlightKeyCharacter() As String
        Get
            Return _HighlightKeyChar
        End Get
        Set(ByVal value As String)
            _HighlightKeyChar = value
        End Set
    End Property


    Private m_Format As String
    Public Property NumericFormat() As String
        Get
            Return m_Format
        End Get
        Set(ByVal value As String)
            m_Format = value
        End Set
    End Property

    Private _ValueScaleFactor As Decimal = 1
    Public Property ScaleFactor() As Decimal
        Get
            Return _ValueScaleFactor
        End Get
        Set(ByVal value As Decimal)
            _ValueScaleFactor = value
        End Set
    End Property
#End Region

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


    Private m_KeypadText As String
    Public Property KeypadText() As String
        Get
            Return m_KeypadText
        End Get
        Set(ByVal value As String)
            m_KeypadText = value
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
    '* Property - Address in PLC to Write Data To
    '*****************************************
    Private m_PLCAddressKeypad As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressKeypad() As String
        Get
            Return m_PLCAddressKeypad
        End Get
        Set(ByVal value As String)
            If m_PLCAddressKeypad <> value Then
                m_PLCAddressKeypad = value
            End If
        End Set
    End Property

    '*****************************************
    '* Property - Address in PLC to Write Data To
    '*****************************************
    Private m_PLCAddressRotate As String = ""
    <System.ComponentModel.Category("PLC Properties")> _
    Public Property PLCAddressRotate() As String
        Get
            Return m_PLCAddressRotate
        End Get
        Set(ByVal value As String)
            If m_PLCAddressRotate <> value Then
                m_PLCAddressRotate = value
            End If
        End Set
    End Property
#End Region

#Region "Events"
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
            If m_CommComponent Is Nothing Then
                Me.Parent.Site.Container.Add(New AdvancedHMIDrivers.EthernetIPforPLCSLCMicroCom)
                m_CommComponent = CType(Me.Parent.Site.Container.Components(Me.Parent.Site.Container.Components.Count - 1), AdvancedHMIDrivers.IComComponent)
            End If


            If DesignMode Then
                If Me.Parent.BackColor = System.Drawing.Color.Black And MyBase.ForeColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ControlText) Then
                    ForeColor = System.Drawing.Color.White
                End If
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

                If KeypadPopUp IsNot Nothing Then
                    KeypadPopUp.Dispose()
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

        MyBase.Text = ErrorMessage
    End Sub


    '**************************************************************************************
    '* Return the text back to its original after displaying the error for a few seconds.
    '**************************************************************************************
    Private Sub ErrorDisplay_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Text = OriginalText

        If ErrorDisplayTime IsNot Nothing Then
            ErrorDisplayTime.Enabled = False
            ErrorDisplayTime.Dispose()
            ErrorDisplayTime = Nothing
        End If
    End Sub
#End Region

#Region "Keypad popup for data entry"
    Private WithEvents KeypadPopUp As MfgControl.AdvancedHMI.Controls.Keypad

    Private Sub KeypadPopUp_ButtonClick(ByVal sender As Object, ByVal e As MfgControl.AdvancedHMI.Controls.KeyPadEventArgs) Handles KeypadPopUp.ButtonClick
        If e.Key = "Quit" Then
            KeypadPopUp.Visible = False
        ElseIf e.Key = "Enter" Then
            If m_CommComponent IsNot Nothing And KeypadPopUp.Value <> "" Then
                If ScaleFactor = 1 Then
                    m_CommComponent.Write(m_PLCAddressKeypad, KeypadPopUp.Value)
                Else
                    m_CommComponent.Write(m_PLCAddressKeypad, KeypadPopUp.Value / ScaleFactor)
                End If
            Else
                DisplayError("CommComponent Property not set")
            End If
            KeypadPopUp.Visible = False
        End If
    End Sub

    '***********************************************************
    '* If labeled is clicked, pop up a keypad for data entry
    '***********************************************************
    Private Sub BasicLabelWithEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Click
        If m_PLCAddressKeypad <> "" And Enabled Then
            If KeypadPopUp Is Nothing Then
                KeypadPopUp = New MfgControl.AdvancedHMI.Controls.Keypad
            End If

            KeypadPopUp.Text = m_KeypadText
            KeypadPopUp.Value = ""
            KeypadPopUp.StartPosition = Windows.Forms.FormStartPosition.CenterScreen
            KeypadPopUp.TopMost = True
            KeypadPopUp.Show()
        End If
    End Sub
#End Region
End Class
