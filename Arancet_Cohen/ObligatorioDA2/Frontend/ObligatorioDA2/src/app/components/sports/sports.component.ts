import { Component, OnInit, ViewChild, Inject } from '@angular/core';
import {MatPaginator, MatSort, MatTableDataSource, MatDialogRef, MAT_DIALOG_DATA, MatDialog} from '@angular/material';
import { Sport } from 'src/app/classes/sport';
import { Globals } from 'src/app/globals';
import { SportsService } from 'src/app/services/sports/sports.service';
import { ConfirmationDialogComponent, DialogInfo } from '../confirmation-dialog/confirmation-dialog';
import { SportDialogComponent } from './sport-dialog/sport-dialog.component';
import { ErrorResponse } from 'src/app/classes/error';
import { ReConnector } from 'src/app/services/auth/reconnector';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sports',
  templateUrl: './sports.component.html',
  styleUrls: ['./sports.component.css']
})
export class SportsComponent implements OnInit {

  displayedColumns: string[] = ['name', 'isTwoTeams', 'options'];
  dataSource:MatTableDataSource<Sport>;
  @ViewChild(MatPaginator) paginator:MatPaginator;
  @ViewChild(MatSort) sort:MatSort;
  errorMessage: string;
  sportEdited: Sport;
  rowEdited: Sport;
  isLoading = false;

  constructor(private router:Router, private reconnector:ReConnector ,private dialog:MatDialog, private sportsService:SportsService) {
    this.getSports();
  }

  ngOnInit() {
  }

  public getSports():void{
    this.isLoading = true;
    this.sportsService.getAllSports().subscribe(
      ((data:Array<Sport>) => this.updateTableData(data)),
      ((error:ErrorResponse) => this.handleSportError(error))
    )
  }

  private updateTableData(sports:Array<Sport>){
    this.dataSource = new MatTableDataSource(sports);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.isLoading = false;
  }

  private handleSportError(error:ErrorResponse) {
    console.log(error);
    if(error.errorCode == 0 || error.errorCode == 401){
      var resultByRef = {result: false};
      this.reconnector.tryReconnect(resultByRef);
      while(!resultByRef.result && this.reconnector.tryCount <= 20){
        //wait
      }
      if(resultByRef.result)
        this.getSports()
      else
        this.router.navigate(['login']);
    }
    this.isLoading = false;
  }
  
  applyFilter(filterValue:string){
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if(this.dataSource.paginator){
      this.dataSource.paginator.firstPage();
    }
  }

  openDeleteDialog(aSport:Sport):void{
    var confirmation:Boolean;
    confirmation = false;
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width:'500px',
      data: {
        confirmation: confirmation,
        title: "Delete " + aSport.name,
        message: "This operation needs confirmation. All data asociated with this sport will be lost. It can not be undone."
      }
    });
    dialogRef.afterClosed().subscribe(
      ((dialgoResponse:DialogInfo) => {
        if(dialgoResponse.confirmation)
          this.performDelete(aSport);
      })
    )
  }

  performDelete(aSport: Sport): void {
    this.sportsService.deleteSport(aSport.name).subscribe(
      ((result:any) => this.updateTableData(this.dataSource.data.filter((s:Sport)=>s.name != aSport.name))),
      ((error:any) => this.handleDeleteError(error, aSport))
    );
  }

  handleDeleteError(error: ErrorResponse, aSport:Sport): void {
    console.log(error);
    if(error.errorCode == 0 || error.errorCode == 401){
      var resultByRef = {result: false};
      this.reconnector.tryReconnect(resultByRef);
      while(!resultByRef.result && this.reconnector.tryCount <= 20){
        //wait
      }
      if(resultByRef.result)
        this.performDelete(aSport);
      else
        this.router.navigate(['login']);
    }
    this.isLoading = false;
  }

  openAddDialog():void{
    var sport = new Sport("");
    const dialogRef = this.dialog.open(SportDialogComponent, {
      width:'500px',
      data: {
        aSport: sport,
        title: "Add new sport",
        isNew: true
      }
    });
    dialogRef.afterClosed().subscribe(
      ((newSport:Sport) => {
        if(newSport != undefined)
          this.performAdd(newSport);
      })
    )
  }

  performAdd(newSport:Sport):void{
    this.dataSource.data.push(newSport);
    this.dataSource._updateChangeSubscription();
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }
}
