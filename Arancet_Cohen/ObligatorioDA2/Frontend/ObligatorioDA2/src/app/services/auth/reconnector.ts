import { AuthService } from "./auth.service";
import { Token } from "../../classes/token";
import { Observable } from "rxjs";
import { Globals } from "../../globals";
import { ErrorResponse } from "../../classes/error";
import { Injectable } from "@angular/core";

@Injectable()
export class ReConnector{

    token:Observable<Token>;
    tryCount:number;
    

    constructor(private auth: AuthService){
        this.tryCount = 0;
    }

    tryReconnect(resultRef:{result:boolean}){
        this.tryCount++;
        const username = Globals.getUsername();
        const password = Globals.getPassword();
        this.token = this.auth.authenticate(username, password); 
        this.token.subscribe(
            ((data:Token) => this.successfulLogin(data, username, password)),
            ((error:ErrorResponse) => this.handleError(error)),
            (()=>{resultRef.result = Globals.isUserLogged()})
        )
    }

    private successfulLogin(tokenResponse:Token, username:string, password:string) {
        Globals.setToken(tokenResponse.token);
        Globals.setLoggedUser(username, password);
        console.log("logueado con exito: " + Globals.getToken());
    }

    private handleError(error:ErrorResponse) {
        if (error.errorCode == 0){
            Globals.logOut();
        }
    }

    private reset(){
        this.tryCount = 0;
    }

}